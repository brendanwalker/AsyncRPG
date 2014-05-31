using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.Web.Cache
{
    public class DictionaryCacheAdapter : ICacheAdapter
    {
        private Dictionary<string, object> m_cache;

        public DictionaryCacheAdapter(Dictionary<string, object> cache)
        {
            m_cache = cache;
        }

        public object this[string name]
        {
            get 
            {
                object result = null;

                if (!m_cache.TryGetValue(name, out result))
                {
                    result = null;
                }

                return result; 
            }

            set 
            { 
                m_cache[name] = value; 
            }
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
            Clear();
        }

    }
}
