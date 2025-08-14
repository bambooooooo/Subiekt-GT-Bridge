namespace SGT_BRIDGE.Models.Order
{
    /// <summary>
    /// Podstawowe dane na fakturze: {NIP}, {Imie i nazwisko}, {Nazwa firmy}
    /// </summary>
    public class OrderInvoice
    {
        string _invoice_nip;
        /// <summary>
        /// NIP do faktury
        /// </summary>
        public string NIP
        {
            get => _invoice_nip;
            set
            {
                // TODO: Sprawdzić, kiedy subiekt nie przepuszcza NIPu do bazy i wyrzuca wyjątek
                _invoice_nip = value;
            }
        }

        /// <summary>
        /// Nazwa firmy (skrócona do 50 znaków)
        /// </summary>
        public string Company_short
        {
            get
            {
                if (_company != null && _company.Trim().Length > 0)
                {
                    string cs = _company.Trim();
                    return cs.Substring(0, Math.Min(cs.Trim().Length, 50));
                }

                return null;
            }
        }

        string _company;
        /// <summary>
        /// Nazwa firmy
        /// </summary>
        public string Company
        {
            get => _company;
            set
            {
                _company = value;
            }
        }

        string _first_name;
        /// <summary>
        /// Imię osoby na fakturze (max 20 znaków)
        /// </summary>
        public string Firstname
        {
            get => _first_name;
            set
            {
                if (value != null && value.Length > 20)
                {
                    _first_name = value.Substring(0, Math.Min(20, value.Length));
                }
                else
                {
                    _first_name = value;
                }
            }
        }

        string _last_name;

        /// <summary>
        /// Nazwisko osoby na fakturze (max 51 znaków)
        /// </summary>
        public string Lastname
        {
            get => _last_name;
            set
            {
                if (value != null && value.Length > 51)
                {
                    string n = value.Substring(0, Math.Min(51, value.Length));
                }
                else
                {
                    _last_name = value;
                }
            }
        }

        /// <summary>
        /// Typ faktury - czy na osobę prywatną, czy na firmę
        /// </summary>
        public OrderInvoiceType InvoiceType { get; private set; }

        /// <summary>
        /// Adres na fakturze
        /// </summary>
        public Address Address { get; set; }

        public OrderInvoice(string nip = null, string firstname = null, string lastname = null, string company = null)
        {
            bool isClientWithInvoice = firstname != null && firstname != "" && lastname != null && lastname != "" && nip != null && nip != "";
            bool isClient = firstname != null && firstname != "" && lastname != null && lastname != "";
            bool isCompany = nip != null && nip != "" && company != null && company != "";
            bool isKnownCompany = nip != null && nip != "";

            if (isClientWithInvoice)
            {
                Firstname = firstname;
                Lastname = lastname;
                NIP = nip;

                InvoiceType = OrderInvoiceType.CLIENT_WITH_NIP;
            }
            else if (isClient)
            {
                Firstname = firstname;
                Lastname = lastname;

                InvoiceType = OrderInvoiceType.CLIENT;
            }
            else if (isCompany)
            {
                Company = company;
                NIP = nip;

                InvoiceType = OrderInvoiceType.COMPANY;
            }
            else if (isKnownCompany)
            {
                NIP = nip;

                InvoiceType = OrderInvoiceType.RECEIVER;
            }
            else
            {
                throw new ArgumentException("Nie można ustalić typu klienta na podstawie podanych danych do faktury");
            }
        }
    }
}
