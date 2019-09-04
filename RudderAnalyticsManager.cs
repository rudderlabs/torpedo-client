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
            //Get access to RudderClient singleton instance
            //The writeKey used here is a sample one generated by the Rudder Labs
            //development team. For Production, the Torpedo development team
            //can retrieve the writeKey from the management web interface and embed
            //in WynnAnalyticsDataConstants.cs
            rudder = RudderClient.getInstance(WynnAnalyticsDataConstants.RUDDER_WRITE_KEY);
            //rudder.enableLog(); //Logging is disabled by default

            //There is no requirement for specifying Amplitude key since same 
            //is configured at Rudder server end

            //Since Rudder SDK supports multiple destinations with different user_id
            //connotations, hence Rudder does not set user_id at instance level
            //It can be included in individual event calls to pass on to Amplitude
            //Same has been incorporated in code below

            GameEngine.LogError("RudderAnalyticsManager: Initialized");
        }

        /// <inheritdoc />
        public bool IsReady { get; private set; }

        /// <inheritdoc />
        public void Init(IAnalyticsManager ownerManager)
        {
            GameEngine.LogError("RudderAnalyticsManager: Init with ownerManager");
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
        public void RecordPurchase(string id, double price, double amountPurchased, string currency = null, string store = null)
        {
            try
            {
                //Every event has an embedded properties structure
                //First we will build the Properties structure
                //Then we will build the encapsulating event structure
                TrackPropertyBuilder propertyBuilder = new TrackPropertyBuilder();
                propertyBuilder.SetCategory("revenue");

                RudderProperty recordPurchaseProperties = propertyBuilder.Build();

                recordPurchaseProperties.AddProperty("productId", id);
                recordPurchaseProperties.AddProperty("price", price);
                recordPurchaseProperties.AddProperty("quantity", 1);
                recordPurchaseProperties.AddProperty("revenueType", store);

                //Add the FoolProofParams
                recordPurchaseProperties.AddProperties(AnalyticsUtils.FoolProofParams(GetCommonEventData()));

                //Now build the event structure
                RudderEventBuilder eventBuilder = new RudderEventBuilder();
                eventBuilder.SetEventName("revenue");

                //Set user id if available
                if (WynnEngine.PlayerId.HasValue())
                {
                    eventBuilder.SetUserId(WynnEngine.PlayerId);
                }

                //Add the properties structure created to the event
                eventBuilder.SetRudderProperty(recordPurchaseProperties);

                //invoke track method
                rudder.Track(eventBuilder);

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
                RudderProperty customProperties = propertyBuilder.Build();
                customProperties.AddProperties(AnalyticsUtils.FoolProofParams(eventData));

                //Now build the event structure
                RudderEventBuilder eventBuilder = new RudderEventBuilder();
                eventBuilder.SetEventName(eventType);

                //Set user id if available
                if (WynnEngine.PlayerId.HasValue())
                {
                    eventBuilder.SetUserId(WynnEngine.PlayerId);
                }

                //Set the user properties
                eventBuilder.SetUserProperty(new RudderUserProperty().AddProperties(GetCommonEventData()));
                
                //Set the event properties
                eventBuilder.SetRudderProperty(customProperties);

                //invoke track method
                rudder.Track(eventBuilder);

                // GameEngine.LogError("RudderAnalyticsManager: Track: " + eventType);
            }
            catch (Exception e)
            {
                GameEngine.LogError("RudderAnalyticsManager: Track: Error: " + e.Message);
            }
        }
    }
}
