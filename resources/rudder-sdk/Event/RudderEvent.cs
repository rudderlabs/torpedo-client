using System;
using UnityEngine;
using System.Collections.Generic;
using com.rudderlabs.unity.library.Event.Property;
using System.Diagnostics;
using com.rudderlabs.unity.library.Errors;

namespace com.rudderlabs.unity.library.Event
{
    public class RudderEvent
    {
        public RudderMessage rl_message = new RudderMessage();
        // API for setting event level integrations
        // Useful if the developer wants to set additional integration platform 
        // for a particular set of events
        public void AddIntegrations(RudderIntegrationPlatform platform, bool isEnabled)
        {
            if (rl_message.rl_integrations.ContainsKey(platform.value))
            {
                rl_message.rl_integrations[platform.value] = isEnabled;
            }
            else
            {
                rl_message.rl_integrations.Add(platform.value, isEnabled);
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

        public RudderEvent()
        {

        }
    }

    public class RudderMessage
    {
        public string rl_channel = "rudder-unity-client";
        public RudderContext rl_context = new RudderContext();
        public string rl_type;
        public string rl_action = "";
        public string rl_message_id = Stopwatch.GetTimestamp().ToString() + "-" + System.Guid.NewGuid().ToString();
        public string rl_timestamp = DateTime.UtcNow.ToString("u");
        public string rl_anonymous_id;
        public string rl_user_id;
        public string rl_event;
        public Dictionary<string, object> rl_properties;
        public Dictionary<string, object> rl_user_properties;
        public Dictionary<string, bool> rl_integrations = new Dictionary<string, bool>();
        public RudderMessage()
        {
            rl_anonymous_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        }
    }

    public class RudderContext
    {
        public RudderApp rl_app = new RudderApp();
        public string rl_platform = Application.platform.ToString();
        public RudderTraits rl_traits = new RudderTraits();
        public RudderLibraryInfo rl_library = new RudderLibraryInfo();
        public RudderOsInfo rl_os = new RudderOsInfo();
        public RudderScreenInfo rl_screen = new RudderScreenInfo();
        public string rl_user_agent = "RudderUnitySdk";
        public string rl_locale = Application.systemLanguage.ToString();
        public RudderDeviceInfo rl_device = new RudderDeviceInfo();
        public RudderNetwork rl_network = new RudderNetwork();
    }

    public class RudderApp
    {
        public string rl_build = Application.productName;
        public string rl_name = Application.productName;
        public string rl_namespace = Application.identifier;
        public string rl_version = Application.version;
    }

    public class RudderLibraryInfo
    {
        public string rl_name = "com.rudderlabs.unity.client";
        public string rl_version = "1.0.0";
    }

    public class RudderOsInfo
    {
        public string rl_name = SystemInfo.operatingSystem.Split()[0];
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

    public class RudderScreenInfo
    {
        public int rl_density = (int)Screen.dpi;
        public int rl_width = Screen.width;
        public int rl_height = Screen.height;
    }

    public class RudderDeviceInfo
    {
        public string rl_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        public string rl_manufacturer = SystemInfo.deviceModel;
        public string rl_model = SystemInfo.deviceModel;
        public string rl_name = SystemInfo.deviceName;
    }

    public class RudderNetwork
    {
        public string rl_carrier = EventRepository.carrier;
    }
}
