using Newtonsoft.Json;
using UnityEngine;

namespace com.rudderlabs.unity.library.Event
{
    public class RudderTraits
    {
        [JsonProperty(PropertyName = "rl_anonymous_id")]
        internal string anonymousId = SystemInfo.deviceUniqueIdentifier.ToLower();
        [JsonProperty(PropertyName = "rl_address")]
        internal TraitsAddress address;
        [JsonProperty(PropertyName = "rl_age")]
        internal string age;
        [JsonProperty(PropertyName = "rl_birthday")]
        internal string birthday;
        [JsonProperty(PropertyName = "rl_company")]
        internal TraitsCompany traitsCompany;
        [JsonProperty(PropertyName = "rl_createdat")]
        internal string createdAt;
        [JsonProperty(PropertyName = "rl_description")]
        internal string description;
        [JsonProperty(PropertyName = "rl_email")]
        internal string email;
        [JsonProperty(PropertyName = "rl_firstname")]
        internal string firstName;
        [JsonProperty(PropertyName = "rl_gender")]
        internal string gender;
        [JsonProperty(PropertyName = "rl_id")]
        internal string id;
        [JsonProperty(PropertyName = "rl_lastname")]
        internal string lastName;
        [JsonProperty(PropertyName = "rl_name")]
        internal string name;
        [JsonProperty(PropertyName = "rl_phone")]
        internal string phone;
        [JsonProperty(PropertyName = "rl_title")]
        internal string title;
        [JsonProperty(PropertyName = "rl_username")]
        internal string userName;

        internal RudderTraits()
        {

        }

        public RudderTraits(TraitsAddress address, string age, string birthday, TraitsCompany traitsCompany, string createdAt, string description, string email, string firstName, string gender, string id, string lastName, string name, string phone, string title, string userName)
        {
            this.address = address;
            this.age = age;
            this.birthday = birthday;
            this.traitsCompany = traitsCompany;
            this.createdAt = createdAt;
            this.description = description;
            this.email = email;
            this.firstName = firstName;
            this.gender = gender;
            this.id = id;
            this.lastName = lastName;
            this.name = name;
            this.phone = phone;
            this.title = title;
            this.userName = userName;
        }
    }

    public class TraitsAddress
    {
        [JsonProperty(PropertyName = "rl_city")]
        internal string city;
        [JsonProperty(PropertyName = "rl_country")]
        internal string country;
        [JsonProperty(PropertyName = "rl_postalcode")]
        internal string postalCode;
        [JsonProperty(PropertyName = "rl_state")]
        internal string state;
        [JsonProperty(PropertyName = "rl_street")]
        internal string street;

        public TraitsAddress(string city, string country, string postalCode, string state, string street)
        {
            this.city = city;
            this.country = country;
            this.postalCode = postalCode;
            this.state = state;
            this.street = street;
        }
    }

    public class TraitsCompany
    {
        [JsonProperty(PropertyName = "rl_name")]
        internal string name;
        [JsonProperty(PropertyName = "rl_id")]
        internal string id;
        [JsonProperty(PropertyName = "rl_industry")]
        internal string industry;

        public TraitsCompany(string name, string id, string industry)
        {
            this.name = name;
            this.id = id;
            this.industry = industry;
        }
    }
}
