using System;
using com.rudderlabs.unity.library.Errors;

namespace com.rudderlabs.unity.library.Event.Property
{
    public class PagePropertyBuilder : RudderPropertyBuilder
    {
        private string title;
        public PagePropertyBuilder SetTitle(string title)
        {
            this.title = title;
            return this;
        }

        private string url;
        public PagePropertyBuilder SetUrl(string url)
        {
            this.url = url;
            return this;
        }

        private string path;
        public PagePropertyBuilder SetPath(string path)
        {
            this.path = path;
            return this;
        }

        private string referrer;
        public PagePropertyBuilder SetReferrer(string referrer)
        {
            this.referrer = referrer;
            return this;
        }

        private string search;
        public PagePropertyBuilder SetSearch(string search)
        {
            this.search = search;
            return this;
        }

        private string keywords;
        public PagePropertyBuilder SetKeywords(string keywords)
        {
            this.keywords = keywords;
            return this;
        }

        public override RudderProperty Build()
        {
            if (url == null)
            {
                throw new RudderException("Key \"url\" is required for track event");
            }

            RudderProperty rudderProperty = new RudderProperty();
            if (title != null)
            {
                rudderProperty.AddProperty("title", title);
            }
            rudderProperty.AddProperty("url", url);
            if (url != null)
            {
                rudderProperty.AddProperty("path", path);
            }
            if (referrer != null)
            {
                rudderProperty.AddProperty("referrer", referrer);
            }
            if (search != null)
            {
                rudderProperty.AddProperty("search", search);
            }
            if(keywords != null)
            {
                rudderProperty.AddProperty("keywords", keywords);
            }
            return rudderProperty;
        }
    }
}
