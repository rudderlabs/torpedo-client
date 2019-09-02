using System;
using UnityEngine;
using System.Collections.Generic;
using com.rudderlabs.unity.library.Event.Property;
using System.Diagnostics;
using com.rudderlabs.unity.library.Errors;

namespace com.rudderlabs.unity.library.Event
{
    [Serializable]
    public class RudderEvent
    {
        // [JsonProperty(PropertyName = "rl_message")]
        [SerializeField]
        public RudderMessage rl_message = new RudderMessage();
        // API for setting event level integrations
        // Useful if the developer wants to set additional integration platform 
        // for a particular set of events
        public void AddIntegrations(RudderIntegrationPlatform platform)
        {
            if (!rl_message.rl_integrations.ContainsKey(platform.value))
            {
                rl_message.rl_integrations.Add(platform.value, true);
            }
        }
        // API for setting event level properties
        // throws exception if "." is present in the key name
        public void SetProperties(Dictionary<string, object> _properties)
        {
            foreach (string key in _properties.Keys)
            {
                if (key.Contains("."))
                {
                    throw new RudderException("\".\" can not be used as a key name for properties");
                }
            }
            rl_message.rl_properties = _properties;
        }

        public void SetProperties(RudderProperty _properties)
        {
            this.SetProperties(_properties.GetPropertyMap());
        }

        public RudderEvent() {
            
        }
    }

    [Serializable]
    public class RudderMessage
    {
        // [JsonProperty(PropertyName = "rl_channel")]
        [SerializeField]
        public string rl_channel = "rudder-unity-client";
        // [JsonProperty(PropertyName = "rl_context")]
        [SerializeField]
        public RudderContext rl_context = new RudderContext();
        // [JsonProperty(PropertyName = "rl_type")]
        [SerializeField]
        public string rl_type;
        // [JsonProperty(PropertyName = "rl_action")]
        [SerializeField]
        public string rl_action = "";
        // [JsonProperty(PropertyName = "rl_message_id")]
        [SerializeField]
        public string rl_message_id = Stopwatch.GetTimestamp().ToString() + "-" + System.Guid.NewGuid().ToString();
        // [JsonProperty(PropertyName = "rl_timestamp")]
        [SerializeField]
        public string rl_timestamp = DateTime.UtcNow.ToString("u");
        // [JsonProperty(PropertyName = "rl_anonymous_id")]
        [SerializeField]
        public string rl_anonymous_id;
        // [JsonProperty(PropertyName = "rl_user_id")]
        [SerializeField]
        public string rl_user_id;
        // [JsonProperty(PropertyName = "rl_event")]
        [SerializeField]
        public string rl_event;
        // [JsonProperty(PropertyName = "rl_properties")]
        [SerializeField]
        public Dictionary<string, object> rl_properties;
        // [JsonProperty(PropertyName = "rl_user_properties")]
        [SerializeField]
        public Dictionary<string, object> rl_user_properties;
        // [JsonProperty(PropertyName = "rl_integrations")]
        [SerializeField]
        public Dictionary<string, bool> rl_integrations = new Dictionary<string, bool>();

        public RudderMessage()
        {
            rl_integrations.Add(RudderIntegrationPlatform.ALL.value, false);
            rl_integrations.Add(RudderIntegrationPlatform.GOOGLE_ANALYTICS.value, true);
            rl_integrations.Add(RudderIntegrationPlatform.AMPLITUDE.value, true);
            rl_anonymous_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        }
    }

    [Serializable]
    public class RudderContext
    {
        [SerializeField]
        // [JsonProperty(PropertyName = "rl_app")]
        public RudderApp rl_app = new RudderApp();
        // [JsonProperty(PropertyName = "rl_platform")]
        [SerializeField]
        public string rl_platform = Application.platform.ToString();
        // [JsonProperty(PropertyName = "rl_traits")]
        [SerializeField]
        public RudderTraits rl_traits = new RudderTraits();
        // [JsonProperty(PropertyName = "rl_library")]
        [SerializeField]
        public RudderLibraryInfo rl_library = new RudderLibraryInfo();
        // [JsonProperty(PropertyName = "rl_os")]
        [SerializeField]
        public RudderOsInfo rl_os = new RudderOsInfo();
        // [JsonProperty(PropertyName = "rl_screen")]
        [SerializeField]
        public RudderScreenInfo rl_screen = new RudderScreenInfo();
        // [JsonProperty(PropertyName = "rl_user_agent")]
        [SerializeField]
        public string rl_user_agent = "RudderUnitySdk";
        // [JsonProperty(PropertyName = "rl_locale")]
        [SerializeField]
        public string rl_locale = Application.systemLanguage.ToString();
        // [JsonProperty(PropertyName = "rl_device")]
        [SerializeField]
        public RudderDeviceInfo rl_device = new RudderDeviceInfo();
        // [JsonProperty(PropertyName = "rl_network")]
        [SerializeField]
        public RudderNetwork rl_network = new RudderNetwork();
    }

    [Serializable]
    public class RudderApp
    {
        // [JsonProperty(PropertyName = "rl_build")]
        [SerializeField]
        public string rl_build = Application.productName;
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name = Application.productName;
        // [JsonProperty(PropertyName = "rl_namespace")]
        [SerializeField]
        public string rl_namespace = Application.identifier;
        // [JsonProperty(PropertyName = "rl_version")]
        [SerializeField]
        public string rl_version = Application.version;
    }

    [Serializable]
    public class RudderLibraryInfo
    {
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name = "com.rudderlabs.unity.client";
        // [JsonProperty(PropertyName = "rl_version")]
        [SerializeField]
        public string rl_version = "1.0.0";
    }

    [Serializable]
    public class RudderOsInfo
    {
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name = SystemInfo.operatingSystem.Split()[0];
        // [JsonProperty(PropertyName = "rl_version")]
        [SerializeField]
        public string rl_version;

        public RudderOsInfo()
        {
#if UNITY_IOS
            rl_version = SystemInfo.operatingSystem.Split()[1];
#else
            rl_version = SystemInfo.operatingSystem.Split()[2];
#endif
        }
    }

    [Serializable]
    public class RudderScreenInfo
    {
        // [JsonProperty(PropertyName = "rl_density")]
        [SerializeField]
        public int rl_density = (int)Screen.dpi;
        // [JsonProperty(PropertyName = "rl_width")]
        [SerializeField]
        public int rl_width = Screen.width;
        // [JsonProperty(PropertyName = "rl_height")]
        [SerializeField]
        public int rl_height = Screen.height;
    }

    [Serializable]
    public class RudderDeviceInfo
    {
        // [JsonProperty(PropertyName = "rl_id")]
        [SerializeField]
        public string rl_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        // [JsonProperty(PropertyName = "rl_manufacturer")]
        [SerializeField]
        public string rl_manufacturer = SystemInfo.deviceModel;
        // [JsonProperty(PropertyName = "rl_model")]
        [SerializeField]
        public string rl_model = SystemInfo.deviceModel;
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name = SystemInfo.deviceName;
    }

    [Serializable]
    public class RudderNetwork
    {
        // [JsonProperty(PropertyName = "rl_carrier")]
        [SerializeField]
        public string rl_carrier = EventRepository.carrier;
    }
}
