using UnityEngine;
using System.Collections;
using LitJson;
using System;
using System.Text;
using System.Collections.Generic;


// Usage:
// AsyncJSONRequest jsonRequest = myGameObj.AddComponent<AsyncJSONRequest>();
// Dictionary<string, object> myJsonParameters = new Dictionary<string, object>();
// myJsonParameters["fieldName1"]= "stringValue";
// myJsonParameters["fieldName2"]= 3.14;
// myJsonParameters["fieldName3"]= 1;
// myJsonParameters["fieldName4"]= true;
// jsonRequest.POST("http://foo.com/MyWebAPI", myJsonParameters, myOnCompleteDelegate)

public class AsyncJSONRequest : MonoBehaviour 
{
    public delegate void RequestListenerDelegate(AsyncJSONRequest request);

	private static string XML_PREFIX = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<string xmlns=\"http://tempuri.org/\">";		
	private static string XML_SUFFIX = "</string>";
	
    public enum eRequestState
    {
	    preflight,
	    pending,
	    failed,
	    succeded
    }

    private string m_requestURL= "";
    private Dictionary<string, object> m_requestData = null;
	private WWWForm m_request= null;
    private WWW m_loader= null;
	private string m_result= null;
		
	private RequestListenerDelegate m_resultListener= null;
    private eRequestState m_state = eRequestState.preflight;
    private string m_failureReason= "";
    private uint m_requestCount= 0;

    public static AsyncJSONRequest Create(GameObject gameObject)
    {
        return gameObject.AddComponent<AsyncJSONRequest>();
    }

    public static void Destroy(AsyncJSONRequest request)
    {
        GameObject.Destroy(request);
    }

    public void POST(string url, Dictionary<string, object> requestData, RequestListenerDelegate onComplete)
    {
        SessionData sessionData= SessionData.GetInstance();
        
        m_requestURL = url;
        m_requestData = requestData;
        m_request = new WWWForm();
        m_resultListener = onComplete;

        if (requestData != null)
        {
            foreach (string fieldName in requestData.Keys)
            {
                string fieldValue = requestData[fieldName].ToString();

                m_request.AddField(fieldName, fieldValue);
            }
        }

        //###HACK:
        // For some reason you can't add a key-value pair directly to the request header 
        // You have to clone it, and add the cookie to that instead
        Hashtable headers = m_request.headers.Clone() as Hashtable;

        if (sessionData.Cookie.Length > 0)
        {
            headers.Add("Cookie", SessionData.GetInstance().Cookie);
        }

        m_loader = new WWW(m_requestURL, m_request.data, headers);

        Debug.Log("Sending POST request");

        StartCoroutine(ExecuteRequest());
    }

    public void GET(string url, RequestListenerDelegate onComplete)
    {
        m_requestURL = url;
        m_requestData = null;
        m_request = null;
        m_resultListener = onComplete;

        m_loader = new WWW(m_requestURL);

        Debug.Log("Sending POST request");

        StartCoroutine(ExecuteRequest());
    }

	public eRequestState GetRequestState()
	{
		return m_state;
	}
		
	public uint GetRequestCount()
	{
		return m_requestCount;
	}
		
	public string GetFailureReason()
	{
		return m_failureReason;
	}

    public Dictionary<string, object> GetRequestData()
	{
		return m_requestData;
	}

    public WWW GetLoader()
    {
        return m_loader;
    }
		
	public JsonData GetResult()
	{
		return JsonMapper.ToObject(m_result);
	}

    public T GetResult<T>()
    {
        return JsonMapper.ToObject<T>(m_result);
    }
				
	private IEnumerator ExecuteRequest()
	{
		m_requestCount++;
		m_state = eRequestState.pending;
		m_result = null;
		m_failureReason = "";

		Debug.Log("Sending Async Request");
		Debug.Log("url: " + m_requestURL);
		if (m_requestData != null)
		{
			Debug.Log("inputJSONData: " + m_requestData.ToString());
		}

        yield return m_loader;

        if (m_loader.error != null && m_loader.error.Length > 0)
        {
			m_state = eRequestState.failed;
			m_failureReason = "loaderError: " + m_loader.error;
            NotifyRequestCompleted();
        }
        else
        {
            LoaderCompleteHandler();
        }
	}
		
	private void NotifyRequestCompleted()
	{
		if (m_state == eRequestState.succeded)
		{
			Debug.Log("Received Async Response(SUCCESS)");
			Debug.Log("url: " + m_requestURL);
				
			if (m_result != null)
			{
				Debug.Log("result: "+m_result.ToString());
			}
		}
		else 
		{
			Debug.LogError("Received Async Response(FAILED)");
			Debug.LogError("url: " + m_requestURL);				
			Debug.LogError("reason: " + m_failureReason);
		}
			
		if (m_resultListener != null)
		{
			m_resultListener(this);
		}
	}	
		
	private void LoaderCompleteHandler()
	{
		try 
		{
			string jsonString = StripXMLWrapper(m_loader.text);				
				
			m_result = jsonString;
			m_state = eRequestState.succeded;				
		}
		catch (Exception error)
		{
			m_state = eRequestState.failed;
			m_failureReason = "JSON Parse Error: " + error.Message;
		}
			
		NotifyRequestCompleted();
	}
				
	private static string StripXMLWrapper(string source)
	{			
		string result = source;

        if (source.IndexOf(XML_PREFIX) == 0 && result.LastIndexOf(XML_SUFFIX) > 0)
		{				
			result = source.Substring(XML_PREFIX.Length, source.Length - XML_PREFIX.Length - XML_SUFFIX.Length);
		}
			
		return result;
	}		
}
