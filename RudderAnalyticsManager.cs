﻿//  -----------------------------------------------------------------------
//  <copyright file="RudderAnalyticsManager.cs" company="Rudder Labs">
//   Copyright (c) 2019 Rudder Labs All rights reserved.
//  </copyright>
//  <author>Rudder Labs</author>
//  -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Com.TorpedoLabs.Propeller.Extensions;
using Com.TorpedoLabs.Wynn;
using Com.TorpedoLabs.Wynn.Backend;
using Rudderlabs;

namespace Com.TorpedoLabs.Propeller.Analytics
{
    public class RudderAnalyticsManager : IAnalyticsLibraryWrapper
    {
        private IAnalyticsManager ownerManager;
        private bool isUserIdSet;
        private RudderClient rudder;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RudderAnalyticsManager()
        {
            // to make it work along with AM or other SDK that uses SQLite
            RudderClient.SerializeSqlite();
            //Get access to RudderClient singleton instance
            //The writeKey used here is a sample one generated by the Rudder Labs
            //development team. For Production, the Torpedo development team
            //can retrieve the writeKey from the management web interface and embed
            //in WynnAnalyticsDataConstants.cs
            RudderConfigBuilder configBuilder = new RudderConfigBuilder()
            .WithEndPointUrl(WynnAnalyticsDataConstants.RUDDER_END_POINT_URL) 
            .WithFactory(RudderAdjustIntegrationFactory.getFactory());
            rudder = RudderClient.GetInstance(
                WynnAnalyticsDataConstants.RUDDER_WRITE_KEY,
                configBuilder.Build()
            );
            //rudder.enableLog(); //Logging is disabled by default

            //There is no requirement for specifying Amplitude key since same 
            //is configured at Rudder server end

            //Since Rudder SDK supports multiple destinations with different user_id
            //connotations, hence Rudder does not set user_id at instance level
            //It can be included in individual event calls to pass on to Amplitude
            //Same has been incorporated in code below

            GameEngine.DebugMsg("RudderAnalyticsManager: Initialized");
        }

        /// <inheritdoc />
        public bool IsReady { get; private set; }

        /// <inheritdoc />
        public void Init(IAnalyticsManager ownerManager)
        {
            GameEngine.DebugMsg("RudderAnalyticsManager: Init with ownerManager");
            this.ownerManager = ownerManager;
        }

        /// <inheritdoc />
        public void ResumeSession()
        {
        }

        /// <inheritdoc />
        public void PauseSession()
        {
        }

        /// <inheritdoc />
        public void Destroy()
        {
        }

        protected Dictionary<string, object> GetCommonEventData()
        {
            if (ownerManager != null)
            {
                return ownerManager.EventsCommonData();
            }

            return new Dictionary<string, object>();
        }

        /// <inheritdoc />
        void RecordPurchase(string id, double price, double amountPurchased, string currency = null, string store = null, string transactionId = null)
        {
            try
            {
                //Every event has an embedded properties structure
                //First we will build the Properties structure
                //Then we will build the encapsulating event structure
                TrackPropertyBuilder propertyBuilder = new TrackPropertyBuilder();
                propertyBuilder.SetCategory("revenue");

                Dictionary<string, object> recordPurchaseProperties = propertyBuilder.Build();

                recordPurchaseProperties.Add("productId", id);
                recordPurchaseProperties.Add("price", price);
                recordPurchaseProperties.Add("quantity", 1);
                if (store != null)
                {
                    recordPurchaseProperties.Add("revenueType", store);
                }
                if (transactionId != null)
                {
                    recordPurchaseProperties.Add("transactionId", transactionId);
                }

                //Add the FoolProofParams
                Dictionary<string, object> eventData = AnalyticsUtils.FoolProofParams(GetCommonEventData());
                foreach (var key in eventData.Keys)
                {
                    var value = eventData[key];
                    if (value != null)
                    {
                        recordPurchaseProperties.Add(key, value);
                    }
                }

                //Now build the event structure
                RudderElementBuilder elementBuilder = new RudderElementBuilder();
                elementBuilder.WithEventName("revenue");

                //Set user id if available
                if (WynnEngine.PlayerId.HasValue())
                {
                    elementBuilder.WithUserId(WynnEngine.PlayerId);
                }

                //Add the properties structure created to the event
                elementBuilder.WithEventProperties(recordPurchaseProperties);

                // Create the event object
                RudderElement element = elementBuilder.Build();

                // Set the integrations
                element.integrations = new Dictionary<string, object>();
                element.integrations.Add("All", true);

                //Invoke track method
                rudder.Track(element);

                // GameEngine.LogError("RudderAnalyticsManager: Track: revenue");
            }
            catch (Exception e)
            {
                GameEngine.LogError("RudderAnalyticsManager: Track: Error: " + e.Message);
            }
        }

        /// <inheritdoc />
        public void RecordCustomEvent(string eventType, Dictionary<string, object> eventData)
        {
            try
            {

                //Every event has an embedded properties structure
                //First we will build the Properties structure
                //Then we will build the encapsulating event structure
                TrackPropertyBuilder propertyBuilder = new TrackPropertyBuilder();
                propertyBuilder.SetCategory(eventType);

                //Now build the properties structure and add the 
                //custom properties received
                Dictionary<string, object> customProperties = propertyBuilder.Build();
                Dictionary<string, object> eventProps = AnalyticsUtils.FoolProofParams(eventData);
                foreach (var key in eventProps.Keys)
                {
                    customProperties.Add(key, eventProps[key]);
                }

                //Now build the event structure
                RudderElementBuilder elementBuilder = new RudderElementBuilder();
                elementBuilder.WithEventName(eventType);

                //Set user id if available
                if (WynnEngine.PlayerId.HasValue())
                {
                    elementBuilder.WithUserId(WynnEngine.PlayerId);
                }

                //Set the user properties
                elementBuilder.WithUserProperties(GetCommonEventData());

                //Set the event properties
                elementBuilder.WithEventProperties(customProperties);

                // Create the event object
                RudderElement element = elementBuilder.Build();

                // Set the integrations
                element.integrations = new Dictionary<string, object>();
                element.integrations.Add("All", true);

                //Invoke track method
                rudder.Track(element);

                // GameEngine.LogError("RudderAnalyticsManager: Track: " + eventType);
            }
            catch (Exception e)
            {
                GameEngine.LogError("RudderAnalyticsManager: Track: Error: " + e.Message);
            }
        }
    }
}
