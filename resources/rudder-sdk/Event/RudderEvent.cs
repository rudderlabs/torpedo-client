using System;
using UnityEngine;
using System.Collections.Generic;
using com.rudderlabs.unity.library.Event.Property;
using Newtonsoft.Json;
using System.Diagnostics;

namespace com.rudderlabs.unity.library.Event
{
    public class RudderEvent
    {
        [JsonProperty(PropertyName = "rl_message")]
        internal RudderMessage message = new RudderMessage();
        // API for setting event level integrations
        // Useful if the developer wants to set additional integration platform 
        // for a particular set of events
        public void AddIntegrations(RudderIntegrationPlatform platform)
        {
            message.integrations.Add(platform.value, true);
        }
        // API for setting event level properties
        // throws exception if "." is present in the key name
        public void SetProperties(Dictionary<string, object> _properties)
        {
            foreach (string key in _properties.Keys)
            {
                if (key.Contains("."))
                {
                    throw RudderException("\".\" can not be used as a key name for properties");
                }
            }
            message.properties = _properties;
        }

        public void SetProperties(RudderProperty _properties)
        {
            this.SetProperties(_properties.GetPropertyMap());
        }

        private Exception RudderException(string v)
        {
            throw new NotImplementedException();
        }
    }

    public class RudderMessage
    {
        [JsonProperty(PropertyName = "rl_channel")]
        internal string channel = "mobile";
        [JsonProperty(PropertyName = "rl_context")]
        internal RudderContext context = new RudderContext();
        [JsonProperty(PropertyName = "rl_type")]
        internal string type;
        [JsonProperty(PropertyName = "rl_action")]
        internal string action = "";
        [JsonProperty(PropertyName = "rl_message_id")]
        internal string messageId = Stopwatch.GetTimestamp().ToString() + "-" + System.Guid.NewGuid().ToString();
        [JsonProperty(PropertyName = "rl_timestamp")]
        internal string timestamp = DateTime.UtcNow.ToString("u");
        [JsonProperty(PropertyName = "rl_anonymous_id")]
        internal string anonymousId;
        [JsonProperty(PropertyName = "rl_user_id")]
        internal string userId;
        [JsonProperty(PropertyName = "rl_event")]
        internal string eventName;
        [JsonProperty(PropertyName = "rl_properties")]
        internal Dictionary<string, object> properties;
        [JsonProperty(PropertyName = "rl_user_properties")]
        internal Dictionary<string, object> userProperty;
        [JsonProperty(PropertyName = "rl_integrations")]
        internal Dictionary<string, bool> integrations = new Dictionary<string, bool>();

        internal RudderMessage()
        {
            integrations.Add(RudderIntegrationPlatform.GOOGLE_ANALYTICS.value, true);
            anonymousId = SystemInfo.deviceUniqueIdentifier.ToLower();
        }
    }

    public class RudderContext
    {
        [JsonProperty(PropertyName = "rl_app")]
        internal RudderApp rudderApp = new RudderApp();
        [JsonProperty(PropertyName = "rl_platform")]
        internal string platform = Application.platform.ToString();
        [JsonProperty(PropertyName = "rl_traits")]
        internal RudderTraits traits = new RudderTraits();
        [JsonProperty(PropertyName = "rl_library")]
        internal RudderLibraryInfo libraryInfo = new RudderLibraryInfo();
        [JsonProperty(PropertyName = "rl_os")]
        internal RudderOsInfo osInfo = new RudderOsInfo();
        [JsonProperty(PropertyName = "rl_screen")]
        internal RudderScreenInfo screenInfo = new RudderScreenInfo();
        [JsonProperty(PropertyName = "rl_user_agent")]
        internal string userAgent = "RudderUnitySdk";
        [JsonProperty(PropertyName = "rl_locale")]
        internal string locale = Application.systemLanguage.ToString();
        [JsonProperty(PropertyName = "rl_device")]
        internal RudderDeviceInfo deviceInfo = new RudderDeviceInfo();
        [JsonProperty(PropertyName = "rl_network")]
        internal RudderNetwork network = new RudderNetwork();
    }

    class RudderApp
    {
        [JsonProperty(PropertyName = "rl_build")]
        internal string build = Application.productName;
        [JsonProperty(PropertyName = "rl_name")]
        internal string name = Application.productName;
        [JsonProperty(PropertyName = "rl_namespace")]
        internal string nameSpace = Application.identifier;
        [JsonProperty(PropertyName = "rl_version")]
        internal string version = Application.version;
    }

    class RudderLibraryInfo
    {
        [JsonProperty(PropertyName = "rl_name")]
        internal string name = "com.rudderlabs.unity.client";
        [JsonProperty(PropertyName = "rl_version")]
        internal string version = "1.0.0";
    }

    class RudderOsInfo
    {
        [JsonProperty(PropertyName = "rl_name")]
        internal string name = SystemInfo.operatingSystem.Split()[0];
        [JsonProperty(PropertyName = "rl_version")]
        internal string version = SystemInfo.operatingSystem.Split()[2];
    }

    class RudderScreenInfo
    {
        [JsonProperty(PropertyName = "rl_density")]
        internal int density = (int)Screen.dpi;
        [JsonProperty(PropertyName = "rl_width")]
        internal int width = Screen.width;
        [JsonProperty(PropertyName = "rl_height")]
        internal int height = Screen.height;
    }

    class RudderDeviceInfo
    {
        [JsonProperty(PropertyName = "rl_id")]
        internal string id = SystemInfo.deviceUniqueIdentifier.ToLower();
        [JsonProperty(PropertyName = "rl_manufacturer")]
        internal string manufacturer = SystemInfo.deviceModel;
        [JsonProperty(PropertyName = "rl_model")]
        internal string model = SystemInfo.deviceModel;
        [JsonProperty(PropertyName = "rl_name")]
        internal string name = SystemInfo.deviceName;
    }

    class RudderNetwork
    {
        [JsonProperty(PropertyName = "rl_carrier")]
        internal string carrier = "unavailable";

        // #if UNITY_IPHONE
        // [DllImport ("__Internal")]
        // private static extern string _GetNetworkInfoIOS();
        // #endif

        // #if UNITY_ANDROID
        // private static extern string _GetNetworkInfoAndroid();
        // #endif

        public RudderNetwork()
        {
            // #if UNITY_IPHONE
            // carrier = _GetNetworkInfoIOS();
            // #endif

            // #if UNITY_ANDROID
            // carrier = _GetNetworkInfoAndroid();
            // #endif
        }
    }
}
