using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web;
using AsyncRPGSharedLib.Web.Interfaces;
using System.Threading;

namespace AsyncRPGSharedLib.Web
{
    public class WebServer
    {
        private Logger m_logger;

        // WebServer State
        private Dictionary<string, Type> m_restModuleTypes;
        private HttpListener m_httpListener;
        private Dictionary<string, object> m_applicationCache;
        private RestSessionManager m_sessionManager;

        // Threading State
        private Thread m_thread;
        private ManualResetEvent m_stopSignal;
        private ManualResetEvent m_exitedSignal;


        public WebServer(
            string UriAddress,
            Logger logger,
            Type[] restModuleTypes)
        {
            m_logger = logger;
            m_applicationCache = new Dictionary<string, object>();
            m_sessionManager = new RestSessionManager();

            m_httpListener = new HttpListener();
            m_httpListener.Prefixes.Add(UriAddress);

            m_restModuleTypes = new Dictionary<string, Type>();
            foreach (Type restModuleType in restModuleTypes)
            {
                System.Attribute[] restModuleAttributes = System.Attribute.GetCustomAttributes(restModuleType);
                string restModuleName = "";

                // See if the module name is specified by a rest module name attribute
                foreach (System.Attribute attribute in restModuleAttributes)
                {
                    if (attribute is RestModuleName)
                    {
                        RestModuleName moduleNameAttribute= (RestModuleName)attribute;

                        restModuleName = moduleNameAttribute.Name;
                        break;
                    }
                }

                // If not, just fall back to the class name
                if (restModuleName.Length == 0)
                {
                    restModuleName = restModuleType.Name;
                }

                // Add the service type suffix
                restModuleName += ".asmx";

                if (!m_restModuleTypes.ContainsKey(restModuleName))
                {
                    m_restModuleTypes.Add(restModuleName, restModuleType);
                    m_logger.LogInfo(string.Format(
                        "Added REST module {0}, mapped to collection name {1}",
                        restModuleType.Name, restModuleName));
                }
                else
                {
                    m_logger.LogError(string.Format(
                        "Failed to add REST module {0}! Another module mapped to collection name {1} already exists",
                        restModuleType.Name, restModuleName));
                }
            }
        }

        public void Start()
        {
            m_httpListener.Start();

            foreach (string prefix in m_httpListener.Prefixes)
            {
                m_logger.LogInfo("AsyncRPGWebServer listening on prefix: " + prefix);
            }

            m_stopSignal = new ManualResetEvent(false);
            m_exitedSignal = new ManualResetEvent(false);

            m_thread = new Thread(() => { this.RequestHandlerLoop(); });
            m_thread.Start();
        }

        public void Stop()
        {
            if (m_thread != null)
            {
                m_stopSignal.Set();
                m_exitedSignal.WaitOne();
                m_thread = null;
            }

            m_httpListener.Stop();
        }

        private void RequestHandlerLoop()
        {
            try
            {
                bool receivedStopSignal = false;

                while (m_httpListener.IsListening && !receivedStopSignal)
                {
                    ProcessRequest();

                    if (m_stopSignal.WaitOne(0))
                    {
                        receivedStopSignal = true;
                    }
                }
            }
            finally
            {
                m_exitedSignal.Set();
            }
        }

        void ProcessRequest()
        {
            IAsyncResult result = m_httpListener.BeginGetContext(ListenerCallback, m_httpListener);

            result.AsyncWaitHandle.WaitOne(500);
        }

        void ListenerCallback(IAsyncResult asyncRequest)
        {
            HttpListenerContext context = m_httpListener.EndGetContext(asyncRequest);
            RestRequest request = new RestRequest(context.Request);
            RestResponse response = new RestResponse(context.Response);
            Type restModuleType = null;
            string result = "";

            bool isNewSession = false;
            RestSession session = m_sessionManager.GetSession(request, out isNewSession);

            m_logger.LogInfo("Server received: " + request.ToString());

            if (m_restModuleTypes.TryGetValue(request.RESTModuleName, out restModuleType))
            {
                // Find the method on the module by name
                MethodInfo method = restModuleType.GetMethod(request.RESTMethodName);

                if (method != null)
                {
                    // Attempt to convert convert all string arguments over to the corresponding native types
                    try
                    {
                        ParameterInfo[] methodParameters= method.GetParameters();
                        object[] parameters = new object[methodParameters.Length];

                        for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                        {
                            Type parameterType = methodParameters[parameterIndex].ParameterType;
                            string parameterName = methodParameters[parameterIndex].Name;
                            string stringParameter = request.RESTMethodParameters[parameterName];

                            parameters[parameterIndex] = Convert.ChangeType(stringParameter, parameterType);
                        }

                        // Attempt to call the REST method
                        try
                        {
                            ICacheAdapter applicationCacheAdapter = new DictionaryCacheAdapter(m_applicationCache);
                            ISessionAdapter sessionAdapter = session;
                            IResponseAdapter responseAdapter = response;

                            RestModule restModule=
                                (RestModule)Activator.CreateInstance(
                                    restModuleType,
                                    applicationCacheAdapter,
                                    sessionAdapter,
                                    responseAdapter);

                            object methodResult = method.Invoke(restModule, parameters);

                            result = methodResult.ToString();
                        }
                        catch (System.Exception ex)
                        {
                            result = "Failed to invoke REST Method: " + ex.Message;
                            m_logger.LogError(result);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        result = "Malformed REST Parameters: " + ex.Message;
                        m_logger.LogError(result);
                    }
                }
                else
                {
                    result = "Unknown REST Method: " + request.RESTMethodName;
                    m_logger.LogError(result);
                }
            }
            else
            {
                result = "Unknown REST Module: " + request.RESTModuleName;
                m_logger.LogError(result);
            }

            // If the module wants the session to be abandoned free it from the session manager
            if (session.IsAbandoned)
            {
                m_sessionManager.FreeSession(session);
            }
            // If this is a new session, give the client the session cookie
            else if (isNewSession)
            {
                response.AppendSessionIdCookie(session);
            }

            // If there was any result string from the rest method, assign it to the result
            if (result.Length > 0)
            {
                response.SetBody(result);
            }

            response.Close();
        }
    }
}
