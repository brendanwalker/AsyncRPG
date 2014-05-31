using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGWebService.Environment
{
    public class HttpSessionAdapter : ISessionAdapter
    {
        private HttpSessionState m_session;

        public HttpSessionAdapter(HttpSessionState cache)
        {
            m_session = cache;
        }

        public object this[string name]
        {
            get { return m_session[name]; }
            set { m_session[name] = value; }
        }

        public void Add(string name, object value)
        {
            m_session[name] = value;
        }

        public void Clear()
        {
            m_session.Clear();
        }

        public void Remove(string name)
        {
            m_session.Remove(name);
        }

        public void RemoveAll()
        {
            m_session.RemoveAll();
        }

        public void Abandon()
        {
            m_session.Abandon();
        }
    }
}