using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace com.rudderlabs.unity.library.Event
{
    internal class RudderEventPayload
    {
        [JsonProperty(PropertyName = "sent_at")]
        public string timestamp = DateTime.UtcNow.ToString("u");
        [JsonProperty(PropertyName = "batch")]
        public List<RudderEvent> events;

        [JsonProperty(PropertyName = "writeKey")]
        public string writeKey;

        [JsonConstructor]
        public RudderEventPayload() {

        }

        internal RudderEventPayload(string _writeKey, List<RudderEvent> _events)
        {
            // Debug.Log("EventRepository: DateTime.UtcNow.ToString(): " + DateTime.UtcNow.ToString("u"));
            events = _events;
            writeKey = _writeKey;
        }
    }
}
