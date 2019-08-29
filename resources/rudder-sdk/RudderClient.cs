using com.rudderlabs.unity.library.Errors;
using com.rudderlabs.unity.library.Event;

namespace com.rudderlabs.unity.library
{
    public class RudderClient
    {
        // singleton instnace of the client
        private static RudderClient instance;
        // instance for event repository (to be used internally)
        private static EventRepository repository;

        private RudderClient() {
            // prevent instance creation through constructor
        }

        // instance initialization method
        public static RudderClient GetInstance(string writeKey)
        {
            return GetInstance(writeKey, Constants.BASE_URL, Constants.FLUSH_QUEUE_SIZE);
        }

        // instance initialization method
        public static RudderClient GetInstance(string writeKey, int flushQueueSize)
        {
            return GetInstance(writeKey, Constants.BASE_URL, flushQueueSize);
        }

        // instance initialization method
        public static RudderClient GetInstance(string writeKey, string endPointUri)
        {
            return GetInstance(writeKey, endPointUri, Constants.FLUSH_QUEUE_SIZE);
        }

        // instance initialization method
        public static RudderClient GetInstance(string writeKey, string endPointUri, int flushQueueSize) {
            if (instance == null)
            {
                instance = new RudderClient();

                repository = new EventRepository(writeKey, flushQueueSize, endPointUri);
            }
            return instance;
        }

        public void enableLog(bool _logging) {
            if (repository == null) {
                throw new RudderException("Client is not initialized");
            }
            repository.enableLogging(_logging);
        }

        // getter & setter for endPointUri
        public string GetEndPointUri()
        {
            return EventRepository.endPointUri;
        }
        public void SetEndPointUri(string _endPointUri)
        {
            EventRepository.endPointUri = _endPointUri;
        }

        // getter & setter for flushQueueSize
        public int GetFlushQueueSize()
        {
            return EventRepository.flushQueueSize;
        }
        public void SetFlushQueueSize(int _flushQueueSize)
        {
            EventRepository.flushQueueSize = _flushQueueSize;
        }

        // end point for track events
        public void Track(RudderEvent rudderEvent)
        {
            rudderEvent.rl_message.rl_type = RudderEventType.TRACK.value;
            repository.Dump(rudderEvent);
        }
        public void Track(RudderEventBuilder builder)
        {
            this.Track(builder.Build());
        }

        // end point for page events
        public void Page(RudderEvent rudderEvent)
        {
            rudderEvent.rl_message.rl_type = RudderEventType.PAGE.value;
            repository.Dump(rudderEvent);
        }
        public void Page(RudderEventBuilder builder)
        {
            this.Page(builder.Build());
        }

        // end point for screen events
        public void Screen(RudderEvent rudderEvent)
        {
            rudderEvent.rl_message.rl_type = RudderEventType.PAGE.value;
            repository.Dump(rudderEvent);
        }
        public void Screen(RudderEventBuilder builder)
        {
            this.Screen(builder.Build());
        }

        // end point for identify calls
        public void Identify(RudderTraits rudderTraits)
        {
            RudderEvent rudderEvent = new RudderEventBuilder()
                .SetEventName("Identify")
                .SetUseId(rudderTraits.rl_id)
                .Build();
            rudderEvent.rl_message.rl_type = RudderEventType.IDENTIFY.value;
            rudderEvent.rl_message.rl_context.rl_traits = rudderTraits;
            repository.Dump(rudderEvent);
        }
        public void Identify(RudderTraitsBuilder builder)
        {
            Identify(builder.Build());
        }

        // end point for flushing events
        public void Flush()
        {
            repository.FlushEventsAsync();
        }
    }
}
