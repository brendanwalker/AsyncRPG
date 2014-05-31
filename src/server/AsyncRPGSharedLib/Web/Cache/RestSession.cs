using System;
using System.Collections.Generic;
using System.Net;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.Web.Cache
{
    public class RestSession : ISessionAdapter
    {
        public bool IsAbandoned { get; private set; }
        public string SessionID { get; private set; }

        private Dictionary<string, object> m_sessionCache;

        public RestSession(string sessionId)
        {
            this.IsAbandoned = false;
            this.SessionID = sessionId;
            this.m_sessionCache = new Dictionary<string, object>();
        }

        // ICacheAdapter
        public object this[string name] 
        {
            get
            {
                object result;

                if (!m_sessionCache.TryGetValue(name, out result))
                {
                    result = null;
                }

                return result;
            }

            set
            {
                m_sessionCache[name] = value;
            }
        }

        public void Add(string name, object value)
        {
            m_sessionCache.Add(name, value);
        }

        public void Clear()
        {
            m_sessionCache.Clear();
        }

        public void Remove(string name)
        {
            m_sessionCache.Remove(name);
        }

        public void RemoveAll()
        {
            m_sessionCache.Clear();
        }

        // ISessionAdapter
        public void Abandon()
        {
            this.IsAbandoned = true;
        }
    }
}
