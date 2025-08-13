using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Adres: ulica, miejscowość, kod pocztowy, kraj
    /// </summary>
    public class Address
    {
        string _street;
        /// <summary>
        /// Ulica i nr domu
        /// </summary>
        [Required]
        public string Street 
        { 
            get => _street; 
            set
            {
                if (value == null || value.Trim().Length <= 0 || value.Trim().Length > 60)
                    throw new ArgumentException($"Address='{value}' field is null");

                _street = value;
            }
        }

        string _postcode;
        /// <summary>
        /// Kod pocztowy
        /// </summary>
        [Required]
        public string Postcode 
        {
            get => _postcode;
            set
            {
                if (value == null || value.Trim().Length <= 0)
                    throw new ArgumentException("Postcode field is null");

                if (value.Length > 8)
                    throw new ArgumentException($"Incorrect post code='{value}'");

                _postcode = value;
            }
        }

        string _city;
        /// <summary>
        /// Miasto
        /// </summary>
        [Required]
        public string City 
        {
            get => _city;
            set
            {
                if (value == null || value.Trim().Length <= 0)
                    throw new ArgumentException("City field is null");

                _city = value;
                _city = _city.Substring(0, Math.Min(40, _city.Length));
            }
        }

        string _country_code;
        /// <summary>
        /// Kod kraju ISO 3166-1 Alpha-2 <br/>
        /// 
        /// </summary>
        [Required]
        public string CountryCode 
        {
            get => _country_code;
            set
            {
                if (value == null)
                    throw new ArgumentException("Country code field is null");

                if (!Utils.Utils.COUNTRY_ISO_2.Contains(value.ToUpper()))
                {
                    throw new ArgumentException($"Invalid country code='{value}'");
                }

                _country_code = value.ToUpper();
            }
        }

        public Address() { }

        public Address(string street, string city, string postcode, string countrycode) 
        {
            Street = street;
            City = city;
            Postcode = postcode;
            CountryCode = countrycode;
        }
    }
}
