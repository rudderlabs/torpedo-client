using System;
using System.Collections.Generic;

namespace com.rudderlabs.unity.library.Event
{
    public class RudderEventPayload
    {
        // [JsonProperty(PropertyName = "sent_at")]
        public string sent_at = DateTime.UtcNow.ToString("u");
        // [JsonProperty(PropertyName = "batch")]
        public List<RudderEvent> batch;

        // [JsonProperty(PropertyName = "writeKey")]
        public string writeKey;

        public RudderEventPayload(string _writeKey, List<RudderEvent> _events)
        {
            // Debug.Log("EventRepository: DateTime.UtcNow.ToString(): " + DateTime.UtcNow.ToString("u"));
            batch = _events;
            writeKey = _writeKey;
        }
    }
}
