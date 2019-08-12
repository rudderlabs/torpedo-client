using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.rudderlabs.unity.library.Event
{
    internal class RudderEventPayload
    {
        [JsonProperty(PropertyName = "sent_at")]
        internal string timestamp;
        [JsonProperty(PropertyName = "batch")]
        internal List<RudderEvent> events;

        [JsonProperty(PropertyName = "writeKey")]
        internal string writeKey;

        internal RudderEventPayload(string _writeKey, List<RudderEvent> _events)
        {
            timestamp = DateTime.UtcNow.ToString("u");
            events = _events;
            writeKey = _writeKey;
        }
    }
}
