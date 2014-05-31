using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Web.Cache;

namespace AsyncRPGSharedLib.Web
{
    public class RestResponse : IResponseAdapter
    {
        private HttpListenerResponse m_response;

        public RestResponse(HttpListenerResponse response)
        {
            m_response = response;

            m_response.StatusCode = (int)HttpStatusCode.OK;
            m_response.StatusDescription = HttpStatusCode.OK.ToString();
        }

        public void AppendSessionIdCookie(RestSession session)
        {
            Cookie cookie = m_response.Cookies[RestSessionManager.SESSION_KEY];

            if (cookie != null)
            {
                cookie.Value = session.SessionID;
            }
            else
            {
                cookie = new Cookie(RestSessionManager.SESSION_KEY, session.SessionID, "/");
                cookie.Expires = DateTime.Now.AddMinutes(RestSessionManager.SESSION_EXPIRATION_MINUTES);
                m_response.Cookies.Add(cookie);
            }
        }

        public void SetBody(string body)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(body);

            m_response.ContentLength64 = buffer.Length;
            m_response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public void Close()
        {
            m_response.Close();
        }

        // IResponseAdapter
        public int StatusCode
        {
            get
            {
                return m_response.StatusCode;
            }

            set
            {
                m_response.StatusCode = value;
            }
        }

        public void AddHeader(string name, string value)
        {
            m_response.AddHeader(name, value);
        }
    }
}
