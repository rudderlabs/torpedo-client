using System;
using System.Collections.Generic;

namespace com.rudderlabs.unity.library.Event.Property
{
    public class RudderProperty
    {
        private Dictionary<string, object> map = new Dictionary<string, object>();

        public Dictionary<string, object> GetPropertyMap()
        {
            return map;
        }

        public bool ContainsKey(string key)
        {
            return map.ContainsKey(key);
        }

        public void AddProperty(string key, object value)
        {
            map[key] = value;
        }

        public void AddProperties(Dictionary<string, object> keyValues)
        {
            foreach (var key in keyValues.Keys)
            {
                AddProperty(key, keyValues[key]);
            }
        }

        public object GetProperty(string key)
        {
            return map[key];
        }
    }
}
