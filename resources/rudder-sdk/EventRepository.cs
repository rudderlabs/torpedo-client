using com.rudderlabs.unity.library.Event;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using LitJson;
using Mono.Data.Sqlite;
using System.Data;
using System.Runtime.InteropServices;

namespace com.rudderlabs.unity.library
{
    internal class EventRepository
    {
        // flag to declare whether the log will be enabled or not
        internal static bool loggingEnabled = true;
        // limit of number of events in a single payload
        internal static int flushQueueSize;
        // writeKey for the instance
        internal static string writeKey;
        // time after which the events will be flushed to the server even if the 
        // flushQueueSize is not reached
        internal static int waitTimeOut;
        // threshold of number of events to be persisted in DB
        internal static int dbRecordCountThreshold;
        // end point uri for the event backend
        internal static string endPointUri { get; set; }
        // sqlite database connection object 
        private static SqliteConnection dbConnection;
        // sqlite database path
        private static string dbPath;

        // carrier persistance variable
        internal static string carrier = "unavailable";
#if (UNITY_IPHONE || UNITY_TVOS)
        // ios plugin method for carrier information
        [DllImport("__Internal")]
        private static extern string _GetiOSCarrierName();
#endif
        // constructor to be called from RudderClient internally.
        // tasks to be performed
        // 1. set the values of writeKey, flushQueueSize, endPointUri, waitTimeOut, dbRecordCountThreshold
        // 2. create database and open database connection
        // 3. create database schema
        // 4. register callback for turning off ssl verification
        // 5. get carrierInformation from respective platform
        // 6. start processor thread
        internal EventRepository(string _writeKey, int _flushQueueSize, string _endPointUri, int _waitTimeOut, int _dbRecordCountThreshold)
        {
            // 1. set the values of writeKey, flushQueueSize, endPointUri, waitTimeOut, dbRecordCountThreshold
            writeKey = _writeKey;
            endPointUri = _endPointUri;
            flushQueueSize = _flushQueueSize;
            waitTimeOut = _waitTimeOut;
            dbRecordCountThreshold = _dbRecordCountThreshold;
            loggingEnabled = false;

            // 2. create database and open database connection
            dbPath = "URI=file:" + Application.persistentDataPath + "/rl_persistance.db";
            if (dbConnection == null)
            {
                CreateDatabaseConnection();
            }

            // 3. create database schema
            CreateSchema();

            // 4. register callback for turning off ssl verification
            ServicePointManager.ServerCertificateValidationCallback = Validator;

            // 5. get carrierInformation from respective platform
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

            // 6. start processor thread
            try
            {
                Thread t = new Thread(ProcessThread);
                t.Start();
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: ProcessThread ERROR: " + ex.Message);
            }
        }
        // create database connection and open connection
        private static void CreateDatabaseConnection()
        {
            try
            {
                dbConnection = new SqliteConnection(dbPath);
                dbConnection.Open();
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: CreateConnection ERROR: " + ex.Message);
            }
        }
        // create database schema
        private void CreateSchema()
        {
            try
            {
                // check if db is accessible
                if (dbConnection == null)
                {
                    CreateDatabaseConnection();
                }

                using (var cmd = dbConnection.CreateCommand())
                {
                    // create table if not exists
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'events' ( " +
                                      "  'id' INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                      "  'event' TEXT NOT NULL, " +
                                      "  'updated' INTEGER NOT NULL" +
                                      ");";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: CreateSchema ERROR: " + ex.Message);
            }
        }
        // event processor
        private void ProcessThread(object obj)
        {
            int sleepCount = 0;
            // infinte loop for processing the events. loop will revive in every one second
            while (true)
            {
                List<int> messageIds = new List<int>();
                List<string> messages = new List<string>();
                // get the record count from database
                int recordCount = GetDBRecordCount();
                // check if persisted event count exceeds threshold
                if (recordCount > dbRecordCountThreshold)
                {
                    // get the events to be deleted form the database 
                    FetchEventsFromDB(messageIds, messages, recordCount - dbRecordCountThreshold);
                    // clear the events
                    ClearEventsFromDB(messageIds);
                }
                // clear the lists for fetching next set of events
                messageIds.Clear();
                messages.Clear();
                // get events from persisted db
                FetchEventsFromDB(messageIds, messages, flushQueueSize);
                // if flushQueueSize if reached or waitTimeOut is reached till last flush
                if (messages.Count >= flushQueueSize || (messages.Count > 0 && sleepCount >= waitTimeOut))
                {
                    // generate stirng payload form message list
                    string payload = GetPayloadFromMessages(messages);
                    if (payload != null)
                    {
                        // send events to server
                        string response = SendEventsToServer(payload);
                        if (response != null)
                        {
                            // if server response is successful remove those events
                            if (response.Equals("OK"))
                            {
                                ClearEventsFromDB(messageIds);
                            }
                            // reset sleepCount to indicate a successful flush
                            sleepCount = 0;
                        }
                    }
                }
                // increment sleepCount to count the time from last successful flush 
                sleepCount += 1;
                Thread.Sleep(1000);
            }
        }
        // get number of events saved in the database
        private int GetDBRecordCount()
        {
            int dbCount = -1;
            try
            {
                // check for database connection
                if (dbConnection == null)
                {
                    CreateDatabaseConnection();
                }
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT count(*) FROM events;";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dbCount = reader.GetInt32(0);
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
        // fetch last `eventCount` number of events from database 
        private void FetchEventsFromDB(List<int> messageIds, List<string> messages, int eventCount)
        {
            try
            {
                // check for datbase connection
                if (dbConnection == null)
                {
                    CreateDatabaseConnection();
                }
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM events ORDER BY updated ASC LIMIT " + eventCount.ToString() + ";";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        messageIds.Add(reader.GetInt32(0));
                        messages.Add(reader.GetString(1));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: FetchEventsFromDB: Error: " + ex.Message);
            }
        }
        // send event payload to server
        private string SendEventsToServer(string payload)
        {
            try
            {
                // check if the endPointUri is formed correctly
                if (!endPointUri.EndsWith("/", StringComparison.Ordinal))
                {
                    endPointUri = endPointUri + "/";
                }
                // create http request object
                var http = (HttpWebRequest)WebRequest.Create(new Uri(endPointUri + "hello"));
                // set content type to "application/json"
                http.ContentType = "application/json";
                // set request method to "POST"
                http.Method = "POST";
                // encode payload
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(payload);
                //write payload to network
                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                //finally close the connection
                newStream.Close();
                // get the response
                var response = http.GetResponse();
                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                // return the response as a string
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: SendEventsToServer: Error: " + ex.Message);
            }
            return null;
        }
        // format payload json from list of individual event json strings
        private string GetPayloadFromMessages(List<string> messages)
        {
            try
            {
                /* 
                    the json payload is created with string builder and string 
                    manipulation to reduce the deserilization. LitJson library 
                    is not working in iOS for desirialization
                 */
                // create the string builder for creating the payload json
                StringBuilder stringBuilder = new StringBuilder();
                // initial token for the json
                stringBuilder.Append("{");
                // add sent_at with a comma
                stringBuilder.Append("\"sent_at\": \"" + DateTime.UtcNow.ToString("u") + "\",");
                // add initial tokens for batch array
                stringBuilder.Append("\"batch\": [");
                // add all the message jsons
                for (int index = 0; index < messages.Count; index++)
                {
                    // add the message
                    stringBuilder.Append(messages[index]);
                    // if not the last one, add a comma
                    if (index != messages.Count - 1)
                    {
                        stringBuilder.Append(",");
                    }
                }
                // add the ending token for batch array
                stringBuilder.Append("],");
                // add write key without the comma as it is the last item in the json
                stringBuilder.Append("\"writeKey\": \"" + writeKey + "\"");
                // add ending token for the json
                stringBuilder.Append("}");

                // finally return the formed json
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: GetPayloadFromMessages: Error: " + ex.Message);
            }
            return null;
        }
        // remove events from database
        private void ClearEventsFromDB(List<int> messageIds)
        {
            try
            {
                // check database connection
                if (dbConnection == null)
                {
                    CreateDatabaseConnection();
                }
                using (var cmd = dbConnection.CreateCommand())
                {
                    // format messageIds csv to be used in query
                    StringBuilder builder = new StringBuilder();
                    for (int index = 0; index < messageIds.Count; index++)
                    {
                        // add the messageId
                        builder.Append(messageIds[index]);
                        // add a comma if not the last item
                        if (index != messageIds.Count - 1)
                        {
                            builder.Append(",");
                        }
                    }
                    string messageIdsString = builder.ToString();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM events WHERE id IN (" + messageIdsString + ");";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: ClearEventsFromDB: Error: " + ex.Message);
            }
        }
        // generic method for dumping all events
        internal void Dump(RudderEvent rudderEvent)
        {
            try
            {
                // create event json from rudder event object
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = false;
                JsonMapper.ToJson(rudderEvent, writer);
                string eventString = writer.ToString();

                // check database connection
                if (dbConnection == null)
                {
                    CreateDatabaseConnection();
                }

                using (var cmd = dbConnection.CreateCommand())
                {
                    // dump event json to database
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
            catch (Exception ex)
            {
                Debug.Log("RudderSDK: Dump: Error" + ex.Message);
            }
        }
        // ssl check validator
        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
