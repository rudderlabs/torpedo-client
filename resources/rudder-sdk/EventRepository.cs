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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using LitJson;

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

            dbPath = "URI=file:" + Application.persistentDataPath + "/rl_persistance.db";
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

            Thread t = new Thread(ProcessThread);
            t.Start();
        }

        private static string SendEventsToServer(string payload)
        {
            if (!endPointUri.EndsWith("/", StringComparison.Ordinal))
            {
                endPointUri = endPointUri + "/";
            }
            // Debug.Log("EventRepository: EndPointUri: in Func: " + endPointUri);
            var http = (HttpWebRequest)WebRequest.Create(new Uri(endPointUri + "hello"));
            http.ContentType = "application/json";
            http.Method = "POST";

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(payload);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        private void FetchEventsFromDB(List<int> messageIds, List<string> messages)
        {

            if (conn == null)
            {
                CreateConnection();
                conn.Open();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM events ORDER BY updated ASC LIMIT " + flushQueueSize.ToString() + ";";

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    messageIds.Add(reader.GetInt32(0));
                    string messageString = reader.GetString(1);
                    messages.Add(messageString);

                    // Debug.Log("EventRepository: fetch: " + messageString);
                }
            }
        }

        private string FormatJsonFromMessages(List<string> messages)
        {
            List<RudderEvent> eventList = new List<RudderEvent>();
            foreach (string message in messages)
            {
                try
                {
                    RudderEvent rlEvent = JsonMapper.ToObject<RudderEvent>(message);
                    // Debug.Log("EventRepository: EVENT RETRIEVED" + rlEvent.rl_message.rl_message_id);
                    eventList.Add(rlEvent);
                }
                catch (Exception e)
                {
                    Debug.Log("EventRepository: EVENT ERROR" + e.Message);
                }
            }

            RudderEventPayload payload = new RudderEventPayload(writeKey, eventList);
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = false;
            JsonMapper.ToJson(payload, writer);
            return writer.ToString();
        }

        private void ProcessThread(object obj)
        {
            int sleepCount = 0;
            while (true)
            {
                List<int> messageIds = new List<int>();
                List<string> messages = new List<string>();
                FetchEventsFromDB(messageIds, messages);
                // Debug.Log("EventRepository: In Loop: " + messages.Count);

                if (messages.Count >= flushQueueSize || (messages.Count > 0 && sleepCount >= 10))
                {
                    string payload = FormatJsonFromMessages(messages);

                    // Debug.Log("EventRepository: PAYLOAD: " + payload);

                    string response = SendEventsToServer(payload);

                    Debug.Log("EventRepository: response: " + response + " | " + messages.Count.ToString());

                    if (response.Equals("OK"))
                    {
                        ClearEventsFromDB(messageIds);
                    }

                    sleepCount = 0;
                }

                sleepCount += 1;

                Thread.Sleep(1000);
            }
        }

        private void ClearEventsFromDB(List<int> messageIds)
        {
            foreach (int messageId in messageIds)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM events WHERE id = @MessageId";

                    cmd.Parameters.Add(new SqliteParameter
                    {
                        ParameterName = "MessageId",
                        Value = messageId
                    });

                    // Debug.Log("EventRepository: delete: " + messageId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal void enableLogging(bool _isEnabled)
        {
            loggingEnabled = _isEnabled;
        }

        private static void CreateConnection()
        {
            // Debug.Log("EventRepository: creating connection");
            try
            {
                conn = new SqliteConnection(dbPath);
            }
            catch (Exception ex)
            {
                Debug.Log("EventRepository DB ERROR: " + ex.Message);
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
                    // Debug.Log("create schema: " + result);
                }
            }
        }

        // generic method for dumping all events
        internal void Dump(RudderEvent rudderEvent)
        {
            try
            {
                // Debug.Log("EventRepository: EVENT DUMPED MESSAGE_ID" + rudderEvent.rl_message.rl_message_id);

                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = false;
                JsonMapper.ToJson(rudderEvent, writer);
                string eventString = writer.ToString();

                // Debug.Log("EventRepository: EVENT DUMPED" + eventString);

                if (conn == null)
                {
                    CreateConnection();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO events (event, updated) VALUES (@Event, @Updated);";
                    cmd.Parameters.Add(new SqliteParameter
                    {
                        ParameterName = "Event",
                        Value = eventString
                    });
                    cmd.Parameters.Add(new SqliteParameter { ParameterName = "Updated", Value = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds });
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.Log("EventRepository: EVENT DUMPED" + e.Message);
            }

        }

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
