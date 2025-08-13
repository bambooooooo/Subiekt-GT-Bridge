using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Dane klienta - email, login, numer telefonu
    /// </summary>
    public class OrderBuyer
    {
        string _buyer_login = "";
        /// <summary>
        /// Login klienta: max 20 znaków
        /// </summary>
        public string Login
        {
            get => _buyer_login;
            set
            {
                if(value == null || value == "")
                {
                    _buyer_login = "";
                }
                else
                {
                    string s = value.Trim();
                    if (s.Length > 20)
                    {
                        s = s.Substring(0, Math.Min(20, s.Length));
                    }
                    
                    _buyer_login = s;
                }
            }
        }

        string _email;
        /// <summary>
        /// Email klienta
        /// </summary>
        public string Email 
        {
            get => _email;
            set
            {
                if (value == null || value.Trim() == "")
                {
                    _email = "";
                }
                else
                {
                    try
                    {
                        MailAddress m = new MailAddress(value);
                        _email = m.Address;
                    }
                    catch (FormatException)
                    {
                        throw new ArgumentException("Email address is incorrect");
                    }
                }
                
            }
        }

        static Regex PHONE_REGEX = new Regex(@"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$");

        string _phone;
        /// <summary>
        /// Numer telefonu klienta
        /// </summary>
        public string Phone 
        {
            get => _phone; 
            set
            {
                if(value == null || value == "")
                {
                    _phone = null;
                }
                else if (!PHONE_REGEX.IsMatch(value))
                {
                    throw new ArgumentException("Invalid phone number");
                }
                else
                {
                    _phone = value;
                }
            }
        }

        string _comment = "";
        /// <summary>
        /// Komentarz klienta do zamówienia. Do 500 znaków
        /// </summary>
        public string Comment
        {
            get
            {
                if (_comment == null)
                    return "";

                return _comment;
            }
            set
            {
                if (value == null || value == "")
                {
                    _comment = "";
                }
                else if (value != null && value.Length > 500)
                {
                    string trimmed = value.Substring(0, Math.Min(500, value.Length));
                    _comment = trimmed;
                }
                else
                {
                    _comment = value;
                }
            }
        }

        public OrderBuyer() { }

        public OrderBuyer(string login = "", string email = "", string phone = "", string comment = "")
        {
            Login = login;
            Email = email;
            Phone = phone;
            Comment = comment;
        }
    }
}
