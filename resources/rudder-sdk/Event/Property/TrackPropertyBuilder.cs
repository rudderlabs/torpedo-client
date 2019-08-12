using System;
using com.rudderlabs.unity.library.Errors;

namespace com.rudderlabs.unity.library.Event.Property
{
    public class TrackPropertyBuilder : RudderPropertyBuilder
    {
        private string category;
        public TrackPropertyBuilder SetCategory(string category)
        {
            this.category = category;
            return this;
        }

        private string label;
        public TrackPropertyBuilder SetLabel(string label)
        {
            this.label = label;
            return this;
        }

        private string value;
        public TrackPropertyBuilder SetValue(string value)
        {
            this.value = value;
            return this;
        }

        public override RudderProperty Build()
        {
            if (category == null)
            {
                throw new RudderException("Key \"category\" is required for track event");
            }

            RudderProperty rudderProperty = new RudderProperty();
            rudderProperty.AddProperty("category", this.category);
            rudderProperty.AddProperty("label", this.label);
            rudderProperty.AddProperty("value", this.value);
            return rudderProperty;
        }
    }
}
