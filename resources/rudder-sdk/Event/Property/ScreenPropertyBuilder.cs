using System;
using com.rudderlabs.unity.library.Errors;

namespace com.rudderlabs.unity.library.Event.Property
{
    public class ScreenPropertyBuilder : RudderPropertyBuilder
    {
        private string name;
        public ScreenPropertyBuilder SetName(string name)
        {
            this.name = name;
            return this;
        }

        public override RudderProperty Build()
        {
            if (name == null)
            {
                throw new RudderException("Key \"name\" is required in properties");
            }
            RudderProperty rudderProperty = new RudderProperty();
            rudderProperty.AddProperty("name", name);
            return rudderProperty;
        }
    }
}
