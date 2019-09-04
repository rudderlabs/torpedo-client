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
        internal static int waitTimeOut;
        internal static int dbRecordCountThreshold;

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

        internal EventRepository(string _writeKey, int _flushQueueSize, string _endPointUri, int _waitTimeOut, int _dbRecordCountThreshold)
        {
            writeKey = _writeKey;
            endPointUri = _endPointUri;
            flushQueueSize = _flushQueueSize;
            waitTimeOut = _waitTimeOut;
            dbRecordCountThreshold = _dbRecordCountThreshold;
            loggingEnabled = false;

            dbPath = "URI=file:" + Application.persistentDataPath + "/rl_persistance.db";
            Debug.Log("RudderSDK: dbPath: " + dbPath);
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
        private static void CreateConnection()
        {
            // Debug.Log("RudderSDK: creating connection");
            try
            {
                conn = new SqliteConnection(dbPath);
                // Debug.Log("RudderSDK: CreateConnection SUCESS: ");
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: CreateConnection ERROR: " + ex.Message);
            }
        }
        private void CreateSchema()
        {
            try
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
                        // Debug.Log("RudderSDK: CreateSchema: Success: " + result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: CreateSchema ERROR: " + ex.Message);
            }

        }

        private void ProcessThread(object obj)
        {
            int sleepCount = 0;
            while (true)
            {
                List<int> messageIds = new List<int>();
                List<string> messages = new List<string>();

                int recordCount = GetDBRecordCount();
                if (recordCount > dbRecordCountThreshold)
                {
                    FetchEventsFromDB(messageIds, messages, recordCount - dbRecordCountThreshold);
                    ClearEventsFromDB(messageIds);
                }

                messageIds.Clear();
                messages.Clear();
                FetchEventsFromDB(messageIds, messages, flushQueueSize);
                // Debug.Log("RudderSDK: In Loop: " + messages.Count);
                if (messages.Count >= flushQueueSize || (messages.Count > 0 && sleepCount >= waitTimeOut))
                {
                    string payload = FormatJsonFromMessages(messages);

                    if (payload != null)
                    {
                        // Debug.Log("RudderSDK: PAYLOAD: " + payload);

                        string response = SendEventsToServer(payload);

                        if (response != null)
                        {
                            Debug.Log("RudderSDK: response: " + response + " | " + messages.Count.ToString());

                            if (response.Equals("OK"))
                            {
                                ClearEventsFromDB(messageIds);
                            }

                            sleepCount = 0;
                        }
                    }
                }

                sleepCount += 1;

                Thread.Sleep(1000);
            }
        }

        private int GetDBRecordCount()
        {
            int dbCount = -1;
            try
            {
                if (conn == null)
                {
                    CreateConnection();
                    conn.Open();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT count(*) FROM events;";

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        dbCount = reader.GetInt32(0);
                        // Debug.Log("RudderSDK: GetDBRecordCount: " + dbCount.ToString());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: FetchEventsFromDB: Error: " + ex.Message);
            }
            return dbCount;
        }

        private void FetchEventsFromDB(List<int> messageIds, List<string> messages, int eventCount)
        {
            try
            {
                if (conn == null)
                {
                    CreateConnection();
                    conn.Open();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM events ORDER BY updated ASC LIMIT " + eventCount.ToString() + ";";

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        messageIds.Add(reader.GetInt32(0));
                        string messageString = reader.GetString(1);
                        messages.Add(messageString);

                        // Debug.Log("RudderSDK: fetch: " + messageString);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: FetchEventsFromDB: Error: " + ex.Message);
            }
        }


        private string SendEventsToServer(string payload)
        {
            try
            {
                if (!endPointUri.EndsWith("/", StringComparison.Ordinal))
                {
                    endPointUri = endPointUri + "/";
                }
                // Debug.Log("RudderSDK: EndPointUri: in Func: " + endPointUri);
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
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: SendEventsToServer: Error: " + ex.Message);
            }
            return null;
        }
        private string FormatJsonFromMessages(List<string> messages)
        {
            try
            {
                List<RudderEvent> eventList = new List<RudderEvent>();
                foreach (string message in messages)
                {
                    try
                    {
                        RudderEvent rlEvent = JsonMapper.ToObject<RudderEvent>(message);
                        // Debug.Log("RudderSDK: EVENT RETRIEVED" + rlEvent.rl_message.rl_message_id);
                        eventList.Add(rlEvent);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("RudderSDK: EVENT ERROR" + e.Message);
                    }
                }

                RudderEventPayload payload = new RudderEventPayload(writeKey, eventList);
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = false;
                JsonMapper.ToJson(payload, writer);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: FormatJsonFromMessages: Error: " + ex.Message);
            }
            return null;
        }
        private void ClearEventsFromDB(List<int> messageIds)
        {
            try
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

                        // Debug.Log("RudderSDK: delete: " + messageId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: ClearEventsFromDB: Error: " + ex.Message);
            }
        }
        internal void enableLogging(bool _isEnabled)
        {
            loggingEnabled = _isEnabled;
        }
        // generic method for dumping all events
        internal void Dump(RudderEvent rudderEvent)
        {
            try
            {
                // Debug.Log("RudderSDK: EVENT DUMPED MESSAGE_ID" + rudderEvent.rl_message.rl_message_id);

                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = false;
                JsonMapper.ToJson(rudderEvent, writer);
                string eventString = writer.ToString();

                // Debug.Log("RudderSDK: EVENT DUMPED" + eventString);

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
                Debug.Log("RudderSDK: EVENT DUMPED" + e.Message);
            }

        }
        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
