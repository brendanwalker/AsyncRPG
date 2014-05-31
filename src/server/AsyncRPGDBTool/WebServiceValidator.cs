using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using System.Web.Script.Serialization;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace AsyncRPGDBTool
{
    class WebServiceValidator
    {
		const string XML_PREFIX = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<string xmlns=\"http://tempuri.org/\">";		
		const string XML_SUFFIX = "</string>";

        private class WebSession
        {
            public string SessionID { get; set; }

            public WebSession()
            {
                SessionID = "";
            }
        }

        private class SessionWebClient
        {
            const string SESSION_KEY = "ASP.NET_SessionId";

            public WebSession Session { get; private set; }

            public SessionWebClient(WebSession session)
            {
                Session = session;
            }

            public string GET(string url)
            {
                string result= "";

                // Create the request
                HttpWebRequest request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
               
                // Retrieve the result
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    result = reader.ReadToEnd();
                }

                return StripXMLWrapper(result);
            }

            public string POST(string url, Dictionary<string, object> parameters)
            {
                HttpWebRequest request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                byte[] formData;
                String result;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                
                if (Session.SessionID.Length > 0)
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(new Cookie(SESSION_KEY, Session.SessionID, "/", request.RequestUri.Host));
                }

                // Save the parameters into the request body
                {
                    StringBuilder parameterString = new StringBuilder();

                    foreach (string fieldName in parameters.Keys)
                    {
                        string fieldValue = parameters[fieldName].ToString();

                        if (parameterString.Length > 0)
                        {
                            parameterString.AppendFormat("&{0}={1}", fieldName, fieldValue);
                        }
                        else
                        {
                            parameterString.AppendFormat("{0}={1}", fieldName, fieldValue);
                        }
                    }

                    formData = UTF8Encoding.UTF8.GetBytes(parameterString.ToString());
                    request.ContentLength = formData.Length;
                }
               
                // Send the request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                }

                // Retrieve the result
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    result = reader.ReadToEnd();

                    // See if we got the cookie with the session id
                    if (response.Headers["Set-Cookie"] != null)
                    {
                        string cookieString = response.Headers["Set-Cookie"];
                        int startIndex = cookieString.IndexOf(SESSION_KEY);

                        if (startIndex != -1)
                        {
                            startIndex += SESSION_KEY.Length+1; // Eat the "SessionID="

                            {
                                int endIndex = cookieString.IndexOf(";", startIndex);

                                endIndex = (endIndex >= 0) ? endIndex - 1 : cookieString.Length - 1;
                                Session.SessionID = cookieString.Substring(startIndex, endIndex - startIndex + 1);
                            }
                        }
                    }
                }

                return StripXMLWrapper(result);
            }
        }

        [Serializable]
        class TestWebServiceScript
        {
            public List<TestWebServiceCall> commands = new List<TestWebServiceCall>();
        }

        [Serializable]
        class TestWebServiceCall
        {
            public const string HTTP_VERB_GET = "GET";
            public const string HTTP_VERB_POST = "POST";

            public string command= "";
            public object queryParameters = null;
            public string[] ignoreFields = new string[] { };
            public object result= null;
        }

        private TextWriter _logger;

        public WebServiceValidator(TextWriter logger)
        {
            _logger = logger;
        }

        public bool ValidateWebService(Command command)
        {
            bool success = true;
            string web_service_url= "";
            string script_path = "";
            TestWebServiceScript testScript= null;

            if (command.HasArgumentWithName("S"))
            {
                script_path= command.GetTypedArgumentByName<CommandArgument_String>("S").ArgumentValue;
            }
            else
            {
                _logger.WriteLine("WebServiceValidator: Missing expected validation script parameter");
                success = false;
            }

            if (command.HasArgumentWithName("W"))
            {
                web_service_url= command.GetTypedArgumentByName<CommandArgument_String>("W").ArgumentValue;
            }
            else
            {
                _logger.WriteLine("WebServiceValidator: ERROR: No WebService URL given");
                success = false;
            }

            // Deserialize the web service script
            if (success)
            {
                success = ParseWebServiceScript(script_path, out testScript);
            }

            if (success)
            {
                WebSession session = new WebSession();
                int command_index = 0;

                foreach (TestWebServiceCall webServiceCall in testScript.commands)
                {
                    if (!VerifyWebServiceCommand(session, web_service_url, webServiceCall, command_index))
                    {
                        success = false;
                        break;
                    }

                    command_index++;
                }
            }

            return success;
        }

        private bool ParseWebServiceScript(
            string script_path,
            out TestWebServiceScript testScript)
        {
            bool success = true;

            try
            {
                System.IO.StreamReader scriptFile = new System.IO.StreamReader(script_path);
                string rawScript = scriptFile.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                scriptFile.Close();
                testScript = serializer.Deserialize<TestWebServiceScript>(rawScript);
            }
            catch (Exception ex)
            {
                _logger.WriteLine("WebServiceValidator: ERROR: Failed to parse script:");
                _logger.WriteLine(script_path);
                _logger.WriteLine(ex.Message);

                testScript = null;
                success = false;
            }

            return success;
        }

        private bool VerifyWebServiceCommand(
            WebSession session,
            string webServiceURL,
            TestWebServiceCall webServiceCall,
            int command_index)
        {
            bool success= true;
            UriBuilder uriBuilder = new UriBuilder(webServiceURL + "/" + webServiceCall.command);
            string fullUrl = uriBuilder.ToString();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonResultString = "";

            _logger.WriteLine("WebServiceValidator: INFO: Sending command #{0}", command_index+1);
            _logger.WriteLine("url: {0}", fullUrl);

            try
            {
                SessionWebClient webClient = new SessionWebClient(session);
                object jsonResultObject= null;

                if (webServiceCall.queryParameters != null)
                {
                    jsonResultString =                     
                        webClient.POST(fullUrl, (Dictionary<string, object>)webServiceCall.queryParameters);
                }
                else
                {
                    jsonResultString =                     
                        webClient.GET(fullUrl);
                }

                jsonResultObject = serializer.DeserializeObject(jsonResultString);

                success = AreObjectsEqual("", webServiceCall.ignoreFields, webServiceCall.result, jsonResultObject);
            }
            catch (System.Exception ex)
            {
                _logger.WriteLine("WebServiceValidator: ERROR: Exception sending command");
                _logger.WriteLine(ex.Message);
                _logger.WriteLine(ex.StackTrace);
                success = false;
            }

            if (success)
            {
                _logger.WriteLine("WebServiceValidator: INFO: Received expected result");               
            }
            else
            {
                _logger.WriteLine("WebServiceValidator: ERROR: Received unexpected result");
            }

            _logger.WriteLine("Result: {0}", jsonResultString);
            _logger.WriteLine();

            return success;
        }

        private bool AreObjectsEqual(
            string path,
            string[] ignoredFields,
            object expectedObject,
            object resultObject)
        {
            bool result= false;

            if (expectedObject == null && resultObject != null)
            {
                _logger.WriteLine("WebServiceValidator: ERROR: expected object is null and result object is not");
                _logger.WriteLine("Path: {0}", path);
            }
            else if (expectedObject != null && resultObject == null)
            {
                _logger.WriteLine("WebServiceValidator: ERROR: expected object is not null and result object is null");
                _logger.WriteLine("Path: {0}", path);

            }
            else if (expectedObject == null && resultObject == null)
            {
                result = true;
            }
            else
            {
                Type expectedType = expectedObject.GetType();
                Type resultType = resultObject.GetType();

                foreach (string ignoredField in ignoredFields)
                {
                    if (ignoredField == path)
                    {
                        result = true;
                        break;
                    }
                }

                if (!result)
                {
                    if (expectedType.GUID == resultType.GUID)
                    {
                        if (CanDirectlyCompare(expectedType))
                        {
                            result = AreValuesEqual(path, expectedObject, resultObject);
                        }
                        else if (expectedType.GUID == typeof(Object[]).GUID)
                        {
                            Object[] expectedList = (Object[])expectedObject;
                            Object[] resultList = (Object[])resultObject;

                            result = true;

                            if (expectedList.Length == resultList.Length)
                            {
                                for (int listIndex = 0; listIndex < expectedList.Length; listIndex++)
                                {
                                    string child_path = string.Format("{0}[{1}]", path, listIndex);

                                    if (!AreObjectsEqual(child_path, ignoredFields, expectedList[listIndex], resultList[listIndex]))
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                _logger.WriteLine("WebServiceValidator: ERROR: Array size mismatch");
                                _logger.WriteLine("Path: {0}", path);
                                _logger.WriteLine("expected array has {0} element(s) while actual array has {1} elements",
                                    expectedList.Length, resultList.Length);
                                result = false;
                            }
                        }
                        else if (expectedType.GUID == typeof(Dictionary<string, object>).GUID)
                        {
                            Dictionary<string, object> expectedDictionary = (Dictionary<string, object>)expectedObject;
                            Dictionary<string, object> resultDictionary = (Dictionary<string, object>)resultObject;

                            result = true;

                            // Verify that all the values that exist in dictionaryA exist in dictionaryB
                            foreach (string key in expectedDictionary.Keys)
                            {
                                string child_path = string.Format("{0}{1}{2}", path, (path.Length > 0) ? "." : "", key);

                                if (resultDictionary.ContainsKey(key))
                                {
                                    if (!AreObjectsEqual(child_path, ignoredFields, expectedDictionary[key], resultDictionary[key]))
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    _logger.WriteLine("WebServiceValidator: ERROR: result object missing expected key");
                                    _logger.WriteLine("Path: {0}", path);
                                    _logger.WriteLine("expected object has key \'{0}\' that result object does not", path);
                                    result = false;
                                    break;
                                }
                            }

                            // Verify that all the values that exist in dictionaryB exist in dictionaryA
                            if (result)
                            {
                                foreach (string key in resultDictionary.Keys)
                                {
                                    string child_path = string.Format("{0}{1}{2}", path, (path.Length > 0) ? "." : "", key);

                                    if (!expectedDictionary.ContainsKey(key))
                                    {
                                        _logger.WriteLine("WebServiceValidator: ERROR: expected object missing result key");
                                        _logger.WriteLine("path: {0}", path);
                                        _logger.WriteLine("result object has key \'{0}\' that expect object does not", path);
                                        result = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.WriteLine("WebServiceValidator: ERROR: Unknown JSON field type:");
                            _logger.WriteLine("Path: {0}", path);
                            _logger.WriteLine("Type: {0}", expectedType.FullName);
                        }
                    }
                    else if (expectedType.GUID == typeof(System.Int32).GUID && resultType.GUID == typeof(System.Decimal).GUID)
                    {
                        Decimal expectedDecimal= (Int32)expectedObject; 
                        Decimal resultDecimal= (Decimal)resultObject;

                        result = AreValuesEqual(path, expectedDecimal, resultDecimal);
                    }
                    else if (expectedType.GUID == typeof(System.Decimal).GUID && resultType.GUID == typeof(System.Int32).GUID)
                    {
                        Decimal expectedDecimal = (Decimal)expectedObject;
                        Decimal resultDecimal = (Int32)resultObject;

                        result = AreValuesEqual(path, expectedDecimal, resultDecimal);
                    }
                    else
                    {
                        _logger.WriteLine("WebServiceValidator: ERROR: Expected type does not match result type:");
                        _logger.WriteLine("Path: {0}", path);
                        _logger.WriteLine("Expected Type: {0}", expectedType.FullName);
                        _logger.WriteLine("Result Type: {0}", resultType.FullName);
                    }
                }
            }

            return result;
        }

        private static bool CanDirectlyCompare(Type type)
        {
            return 
                typeof(IComparable).IsAssignableFrom(type) ||
                type.IsPrimitive || 
                type.IsValueType;
        }

        private bool AreValuesEqual(
            string path,
            object expectedValue, 
            object resultValue)
        {
            bool result;
            IComparable selfValueComparer;

            selfValueComparer = expectedValue as IComparable;

            if (expectedValue == null && resultValue != null || expectedValue != null && resultValue == null)
                result = false; // one of the values is null
            else if (selfValueComparer != null && selfValueComparer.CompareTo(resultValue) != 0)
                result = false; // the comparison using IComparable failed
            else if (!object.Equals(expectedValue, resultValue))
                result = false; // the comparison using Equals failed
            else
                result = true; // match

            if (!result)
            {
                _logger.WriteLine("WebServiceValidator: ERROR: expected value does not match result value");
                _logger.WriteLine("Path: {0}", path);
                _logger.WriteLine("Expected: {0}", expectedValue.ToString());
                _logger.WriteLine("Result: {0}", resultValue.ToString());
            }

            return result;
        }

		private static string StripXMLWrapper(string source) 
		{			
			string result = source;
			
			if (source.IndexOf(XML_PREFIX) == 0)
			{
				int end_index = result.LastIndexOf(XML_SUFFIX);
				
				result = source.Substring(XML_PREFIX.Length, source.Length - XML_PREFIX.Length - XML_SUFFIX.Length);
			}
			
			return result;
		}
    }
}
