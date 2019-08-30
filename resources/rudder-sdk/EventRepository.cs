using com.rudderlabs.unity.library.Event;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.IO;
using Mono.Data.Sqlite;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace com.rudderlabs.unity.library
{
    internal class EventRepository
    {
        internal static bool loggingEnabled = true;
        internal static int flushQueueSize;
        internal static string writeKey;
        internal static string endPointUri { get; set; }
        // internal buffer for events which will be cleared upon successful transmission of events to server
        private List<RudderEvent> eventBuffer = new List<RudderEvent>();

        private static SqliteConnection conn;

        private static string dbPath;

        private int totalEvents = 0;

        internal static string carrier = "unavailable";

#if (UNITY_IPHONE || UNITY_TVOS)
        [DllImport("__Internal")]
        private static extern string _GetiOSCarrierName();
#endif

        internal EventRepository(string _writeKey, int _flushQueueSize, string _endPointUri)
        {
            writeKey = _writeKey;
            endPointUri = _endPointUri;
            flushQueueSize = _flushQueueSize;
            loggingEnabled = false;

            dbPath = "URI=file:" + Application.persistentDataPath + "/persistance.db";
            if (conn == null)
            {
                CreateConnection();
            }

            ServicePointManager.ServerCertificateValidationCallback = Validator;

            totalEvents = 0;

            // make a cache of carrier information
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass ajc = new AndroidJavaClass("com.rudderlabs.rudderandroidplugin.Helper");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            carrier = ajc.CallStatic<string>("getCarrier", context);
#endif

#if (UNITY_IPHONE || UNITY_TVOS)
            carrier = _GetiOSCarrierName();
#endif
            CreateSchema();
        }

        internal void enableLogging(bool _isEnabled)
        {
            loggingEnabled = _isEnabled;
        }

        private static void CreateConnection()
        {
            Debug.Log("EventRepository: creating connection");
            try
            {
                conn = new SqliteConnection(dbPath);
            }
            catch (Exception ex)
            {
                Debug.Log("EventRepository DB ERROR: " + ex);
            }
        }

        private void CreateSchema()
        {
            if (conn == null)
            {
                CreateConnection();
            }

            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'events' ( " +
                                  "  'id' INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                  "  'event' TEXT NOT NULL, " +
                                  "  'updated' INTEGER NOT NULL" +
                                  ");";
                var result = cmd.ExecuteNonQuery();
                if (loggingEnabled)
                {
                    Debug.Log("create schema: " + result);
                }
            }
        }

        // generic method for dumping all events
        internal void Dump(RudderEvent rudderEvent)
        {
            Debug.Log("EVENT DUMPED");
            // temporary for torpedo only as torpedo is specific to amplitude
            rudderEvent.AddIntegrations(RudderIntegrationPlatform.AMPLITUDE);
            // add incoming event to buffer 
            eventBuffer.Add(rudderEvent);

            // if flushQueueSize is full flush the events to server
            if (eventBuffer.Count == flushQueueSize)
            {
                Debug.Log("EVENT FLUSH STARTED");
                totalEvents += eventBuffer.Count;
                FlushEventsAsync();
            }
        }

        internal void FlushEventsAsync()
        {
            try
            {
                // TOTAL EVENT Count
                Debug.Log(
                    "\n++++++++++++++++++++++++++++++++++++++++++++" +
                    "\nTOTAL EVENT COUNT: " + totalEvents +
                    "\n++++++++++++++++++++++++++++++++++++++++++++"
                );


                // consturuct payload with "sent_at" and "batch" 
                RudderEventPayload eventPayload = new RudderEventPayload(writeKey, eventBuffer);

                Debug.Log("EventRepository: FlushEventsAsync: " + eventPayload.sent_at);
                Debug.Log("EventRepository: FlushEventsAsync: " + eventPayload.batch.Count);
                Debug.Log("EventRepository: FlushEventsAsync: " + eventPayload.writeKey);

                // serialize payload to JSON string
                // string payloadString = JsonConvert.SerializeObject(eventPayload,
                //                                                 Formatting.None,
                //                                                 new JsonSerializerSettings
                //                                                 {
                //                                                     NullValueHandling = NullValueHandling.Ignore
                //                                                 });
                string payloadString = JsonUtility.ToJson(eventPayload);
                // string payloadString = new JavaScriptSerializer().Serialize(eventPayload);

                Debug.Log("EventRepository: Payload String: " + payloadString);

                // make network request to flush the events 
                PostEventToServer(payloadString);
                Debug.Log("EventRepository : event posted");
                // empty buffer
                eventBuffer.RemoveRange(0, eventBuffer.Count);
            }
            catch (Exception e)
            {
                Debug.Log("EventRepository: FlushEventsAsync: Error: " + e);
            }

        }

        private static List<string> payloadBuffer = new List<string>();
        private static void PostEventToServer(string payload)
        {
            payloadBuffer.Add(payload);
            ThreadStart jobRef = new ThreadStart(ProcessEvent);
            Thread processorThread = new Thread(jobRef);
            processorThread.Start();
        }

        private static void ProcessEvent()
        {
            try
            {
                if (payloadBuffer.Count == 0)
                {
                    return;
                }
                string payload = payloadBuffer[0];
                payloadBuffer.RemoveAt(0);

                Debug.Log("EventRepository : event db dump started");
                // dump events to persistance DB first
                PersistEvents(payload);
                Debug.Log("EventRepository : event db dump completed");

                Debug.Log("EventRepository : event network dump started");
                // flush Events from DB to server
                SendEventsToServer();
                Debug.Log("EventRepository : event network dump completed");
            }
            catch (Exception e)
            {
                Debug.Log("EventRepository: PostEventToServer: Error: " + e);
            }
        }

        private static void SendEventsToServer()
        {
            Debug.Log("EventRepository : event network dump in func started");
            if (conn == null)
            {
                CreateConnection();
                conn.Open();
            }
            string payload = null;
            int batchId = -1;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM events ORDER BY updated ASC LIMIT 1;";

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    batchId = reader.GetInt32(0);
                    payload = reader.GetString(1);
                    break;
                }
            }

            if (payload == null || batchId == -1)
            {
                Debug.Log("EventRepository : payloadL: " + payload);
                Debug.Log("EventRepository : batchIdL: " + batchId);
                return;
            }
            if (!endPointUri.EndsWith("/"))
            {
                endPointUri = endPointUri + "/";
            }
            Debug.Log("EventRepository: EndPointUri: in Func: " + endPointUri);
            string content = "";

            // var postData = System.Text.Encoding.UTF8.GetBytes(payload);
            // WebRequest req = WebRequest.Create(endPointUri);
            // req.Method = 
            // WebRequest req = WebRequest.Post(endPointUri, payload);
            // req.SetRequestHeader("Content-Type", "application/json");
            // Stream stream = req.GetResponse ().GetResponseStream ();

            var http = (HttpWebRequest)WebRequest.Create(new Uri(endPointUri + "hello"));
            http.ContentType = "application/json";
            http.Method = "POST";

            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(payload);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            content = sr.ReadToEnd();

            if (content.Equals("OK"))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM events WHERE id = @BatchId";

                    cmd.Parameters.Add(new SqliteParameter
                    {
                        ParameterName = "BatchId",
                        Value = batchId
                    });

                    var result = cmd.ExecuteNonQuery();
                }
            }

            SendEventsToServer();
        }

        private static void PersistEvents(string payload)
        {

            Debug.Log("EventRepository: persistance task started");
            if (conn == null)
            {
                CreateConnection();
            }
            using (var cmd = conn.CreateCommand())
            {
                Debug.Log("EventRepository: db insertion started");
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO events (event, updated) " +
                                  "VALUES (@Event, @Updated);";

                cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "Event",
                    Value = payload
                });

                cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "Updated",
                    Value = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
                });

                var result = cmd.ExecuteNonQuery();
                Debug.Log("EventRepository: insert query" + cmd.ToString());
                Debug.Log("EventRepository: insert event: " + result);
            }
        }

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
