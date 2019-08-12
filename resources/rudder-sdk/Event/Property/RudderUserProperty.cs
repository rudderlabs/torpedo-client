using System.Collections.Generic;

namespace com.rudderlabs.unity.library.Event.Property
{
    public class RudderUserProperty
    {
        private Dictionary<string, object> map = new Dictionary<string, object>();

        internal Dictionary<string, object> GetPropertyMap() {
            return map;
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