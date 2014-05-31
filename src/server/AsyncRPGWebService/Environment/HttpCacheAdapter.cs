using System;
using System.Collections.Generic;
using System.Web;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGWebService.Environment
{
    public class HttpCacheAdapter : ICacheAdapter
    {
        private HttpApplicationState m_cache;

        public HttpCacheAdapter(HttpApplicationState cache)
        {
            m_cache = cache;
        }

        public object this[string name]
        {
            get { return m_cache[name]; }
            set { m_cache[name] = value; } 
        }

        public void Add(string name, object value)
        {
            m_cache[name] = value;
        }

        public void Clear()
        {
            m_cache.Clear();
        }

        public void Remove(string name)
        {
            m_cache.Remove(name);
        }

        public void RemoveAll()
        {
            m_cache.RemoveAll();
        }
    }
}