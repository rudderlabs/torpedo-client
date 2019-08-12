namespace com.rudderlabs.unity.library.Event
{
    public class RudderEventType
    {
        private RudderEventType(string _value)
        {
            value = _value;
        }

        public string value { get; private set; }

        public static RudderEventType TRACK { get { return new RudderEventType("track"); } }
        public static RudderEventType PAGE { get { return new RudderEventType("page"); } }
        public static RudderEventType SCREEN { get { return new RudderEventType("screen"); } }
        public static RudderEventType IDENTIFY { get { return new RudderEventType("identify"); } }
    }
}