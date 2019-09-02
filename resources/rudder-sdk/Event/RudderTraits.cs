using System;
using UnityEngine;

namespace com.rudderlabs.unity.library.Event
{
    [Serializable]
    public class RudderTraits
    {
        // [JsonProperty(PropertyName = "rl_anonymous_id")]
        [SerializeField]
        public string rl_anonymous_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        // [JsonProperty(PropertyName = "rl_address")]
        [SerializeField]
        public TraitsAddress rl_address;
        // [JsonProperty(PropertyName = "rl_age")]
        [SerializeField]
        public string rl_age;
        // [JsonProperty(PropertyName = "rl_birthday")]
        [SerializeField]
        public string rl_birthday;
        // [JsonProperty(PropertyName = "rl_company")]
        [SerializeField]
        public TraitsCompany rl_company;
        // [JsonProperty(PropertyName = "rl_createdat")]
        [SerializeField]
        public string rl_createdat;
        // [JsonProperty(PropertyName = "rl_description")]
        [SerializeField]
        public string rl_description;
        // [JsonProperty(PropertyName = "rl_email")]
        [SerializeField]
        public string rl_email;
        // [JsonProperty(PropertyName = "rl_firstname")]
        [SerializeField]
        public string rl_firstname;
        // [JsonProperty(PropertyName = "rl_gender")]
        [SerializeField]
        public string rl_gender;
        // [JsonProperty(PropertyName = "rl_id")]
        [SerializeField]
        public string rl_id;
        // [JsonProperty(PropertyName = "rl_lastname")]
        [SerializeField]
        public string rl_lastname;
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name;
        // [JsonProperty(PropertyName = "rl_phone")]
        [SerializeField]
        public string rl_phone;
        // [JsonProperty(PropertyName = "rl_title")]
        [SerializeField]
        public string rl_title;
        // [JsonProperty(PropertyName = "rl_username")]
        [SerializeField]
        public string rl_username;

        public RudderTraits()
        {

        }

        public RudderTraits(TraitsAddress address, string age, string birthday, TraitsCompany traitsCompany, string createdAt, string description, string email, string firstName, string gender, string id, string lastName, string name, string phone, string title, string userName)
        {
            this.rl_address = address;
            this.rl_age = age;
            this.rl_birthday = birthday;
            this.rl_company = traitsCompany;
            this.rl_createdat = createdAt;
            this.rl_description = description;
            this.rl_email = email;
            this.rl_firstname = firstName;
            this.rl_gender = gender;
            this.rl_id = id;
            this.rl_lastname = lastName;
            this.rl_name = name;
            this.rl_phone = phone;
            this.rl_title = title;
            this.rl_username = userName;
        }
    }

    [Serializable]
    public class TraitsAddress
    {
        // [JsonProperty(PropertyName = "rl_city")]
        [SerializeField]
        public string rl_city;
        // [JsonProperty(PropertyName = "rl_country")]
        [SerializeField]
        public string rl_country;
        // [JsonProperty(PropertyName = "rl_postalcode")]
        [SerializeField]
        public string rl_postalcode;
        // [JsonProperty(PropertyName = "rl_state")]
        [SerializeField]
        public string rl_state;
        // [JsonProperty(PropertyName = "rl_street")]
        [SerializeField]
        public string rl_street;

        public TraitsAddress()
        {

        }

        public TraitsAddress(string city, string country, string postalCode, string state, string street)
        {
            this.rl_city = city;
            this.rl_country = country;
            this.rl_postalcode = postalCode;
            this.rl_state = state;
            this.rl_street = street;
        }
    }

    [Serializable]
    public class TraitsCompany
    {
        // [JsonProperty(PropertyName = "rl_name")]
        [SerializeField]
        public string rl_name;
        // [JsonProperty(PropertyName = "rl_id")]
        [SerializeField]
        public string rl_id;
        // [JsonProperty(PropertyName = "rl_industry")]
        [SerializeField]
        public string rl_industry;

        public TraitsCompany()
        {

        }

        public TraitsCompany(string name, string id, string industry)
        {
            this.rl_name = name;
            this.rl_id = id;
            this.rl_industry = industry;
        }
    }
}
