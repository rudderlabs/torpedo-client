using System;
namespace com.rudderlabs.unity.library.Event
{
    public class RudderTraitsBuilder
    {
        private string city;
        public RudderTraitsBuilder SetCity(string city)
        {
            this.city = city;
            return this;
        }

        private string country;
        public RudderTraitsBuilder SetCountry(string country)
        {
            this.country = country;
            return this;
        }

        private string postalCode;
        public RudderTraitsBuilder SetPostalCode(string postalCode)
        {
            this.postalCode = postalCode;
            return this;
        }

        private string state;
        public RudderTraitsBuilder SetState(string state)
        {
            this.state = state;
            return this;
        }

        private string street;
        public RudderTraitsBuilder SetStreet(string street)
        {
            this.street = street;
            return this;
        }

        private int age;
        public RudderTraitsBuilder SetAge(int age)
        {
            this.age = age;
            return this;
        }

        private string birthday;
        public RudderTraitsBuilder SetBirthday(string birthday)
        {
            this.birthday = birthday;
            return this;
        }

        private string companyName;
        public RudderTraitsBuilder SetCompanyName(string companyName)
        {
            this.companyName = companyName;
            return this;
        }

        private string companyId;
        public RudderTraitsBuilder SetCompanyId(string companyId)
        {
            this.companyId = companyId;
            return this;
        }

        private string industry;
        public RudderTraitsBuilder SetIndustry(string industry)
        {
            this.industry = industry;
            return this;
        }

        private string createdAt;
        public RudderTraitsBuilder SetCreatedAt(string createdAt)
        {
            this.createdAt = createdAt;
            return this;
        }

        private string description;
        public RudderTraitsBuilder SetDescription(string description)
        {
            this.description = description;
            return this;
        }

        private string email;
        public RudderTraitsBuilder SetEmail(string email)
        {
            this.email = email;
            return this;
        }

        private string firstName;
        public RudderTraitsBuilder SetFirstName(string firstName)
        {
            this.firstName = firstName;
            return this;
        }

        private string gender;
        public RudderTraitsBuilder SetGender(string gender)
        {
            this.gender = gender;
            return this;
        }

        private string id;
        public RudderTraitsBuilder SetId(string id)
        {
            this.id = id;
            return this;
        }

        private string lastName;
        public RudderTraitsBuilder SetLastName(string lastName)
        {
            this.lastName = lastName;
            return this;
        }

        private string name;
        public RudderTraitsBuilder SetName(string name)
        {
            this.name = name;
            return this;
        }

        private string phone;
        public RudderTraitsBuilder SetPhone(string phone)
        {
            this.phone = phone;
            return this;
        }

        private string title;
        public RudderTraitsBuilder SetTitle(string title)
        {
            this.title = title;
            return this;
        }

        private string userName;
        public RudderTraitsBuilder SetUserName(string userName)
        {
            this.userName = userName;
            return this;
        }

        public RudderTraits Build()
        {
            return new
                RudderTraits(
                new TraitsAddress(
                    this.city,
                    this.country,
                    this.postalCode,
                    this.state,
                    this.street),
                this.age.ToString(),
                this.birthday,
                new TraitsCompany(
                    this.companyName,
                    this.companyId,
                    this.industry),
                this.createdAt,
                this.description,
                this.email,
                this.firstName,
                this.gender,
                this.id,
                this.lastName,
                this.name,
                this.phone,
                this.title,
                this.userName);
        }
    }
}
