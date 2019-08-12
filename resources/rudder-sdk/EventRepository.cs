using com.rudderlabs.unity.library.Event;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Net;

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

            totalEvents = 0;

            CreateSchema();
        }

        internal void enableLogging(bool _isEnabled) {
            loggingEnabled = _isEnabled;
        }

        private static void CreateConnection()
        {
            Debug.Log("EventRepository: creating connection");
            conn = new SqliteConnection(dbPath);
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
            // temporary for torpedo only as torpedo is specific to amplitude
            rudderEvent.AddIntegrations(RudderIntegrationPlatform.AMPLITUDE);
            // add incoming event to buffer 
            eventBuffer.Add(rudderEvent);

            // if flushQueueSize is full flush the events to server
            if (eventBuffer.Count == flushQueueSize)
            {
                totalEvents += eventBuffer.Count;
                FlushEventsAsync();
            }
        }

        internal void FlushEventsAsync()
        {
            // consturuct payload with "sent_at" and "batch" 
            RudderEventPayload eventPayload = new RudderEventPayload(writeKey, eventBuffer);

            // serialize payload to JSON string
            string payloadString = JsonConvert.SerializeObject(eventPayload,
                                                            Formatting.None,
                                                            new JsonSerializerSettings
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });

            // TOTAL EVENT Count
            Debug.Log(
                "\n++++++++++++++++++++++++++++++++++++++++++++" +
                "\nTOTAL EVENT COUNT: " + totalEvents +
                "\n++++++++++++++++++++++++++++++++++++++++++++"
            );

            // make network request to flush the events 
            PostEventToServer(payloadString);
            Debug.Log("EventRepository : event posted");
            // empty buffer
            eventBuffer.RemoveRange(0, eventBuffer.Count);
        }

        private static void PostEventToServer(string payload)
        {
            Task.Run(() =>
            {
                Debug.Log("EventRepository : event db dump started");
                // dump events to persistance DB first
                PersistEvents(payload);
                Debug.Log("EventRepository : event db dump completed");

                Debug.Log("EventRepository : event network dump started");
                // flush Events from DB to server
                SendEventsToServer();
                Debug.Log("EventRepository : event network dump completed");
            });
        }

        private static async void SendEventsToServer()
        {
            Debug.Log("EventRepository : event network dump in func started");
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, endPointUri + "/hello"))
                {
                    Debug.Log("EventRepository : client and request present");
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
                        Debug.Log("EventRepository : payload, batchId null");
                        return;
                    }

                    if (loggingEnabled)
                    {
                        Debug.Log("EventRepository : REQUEST: " + payload);
                    }
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                    if (loggingEnabled)
                    {
                        Debug.Log("EventRepository : REQUEST LENGTH: " + request.Content.ToString());
                    }
                    HttpResponseMessage response = await client.SendAsync(request);
                    string responseString = await response.Content.ReadAsStringAsync();
                    HttpStatusCode statusCode = response.StatusCode;
                    if (loggingEnabled)
                    {
                        Debug.Log("EventRepository : RESPONSE STATUS_CODE: " + statusCode);
                        Debug.Log("EventRepository : RESPONSE BODY: " + responseString);
                    }

                    if (statusCode == HttpStatusCode.OK)
                    {
                        if (loggingEnabled)
                        {
                            Debug.Log("EventRepository : EVENTS FLUSHED");
                        }

                        // remove batch from local DB
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

                        // call again for more events
                        SendEventsToServer();
                    }
                }
            }
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
    }
}
