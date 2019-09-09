using System;
using UnityEngine;

namespace com.rudderlabs.unity.library.Event
{
    public class RudderTraits
    {
        public string rl_anonymous_id = SystemInfo.deviceUniqueIdentifier.ToLower();
        public TraitsAddress rl_address;
        public string rl_age;
        public string rl_birthday;
        public TraitsCompany rl_company;
        public string rl_createdat;
        public string rl_description;
        public string rl_email;
        public string rl_firstname;
        public string rl_gender;
        public string rl_id;
        public string rl_lastname;
        public string rl_name;
        public string rl_phone;
        public string rl_title;
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

    public class TraitsAddress
    {
        public string rl_city;
        public string rl_country;
        public string rl_postalcode;
        public string rl_state;
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

    public class TraitsCompany
    {
        public string rl_name;
        public string rl_id;
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
