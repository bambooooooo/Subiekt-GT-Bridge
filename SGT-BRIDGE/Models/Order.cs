using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models
{
    public class Order
    {
        string _number;
        /// <summary>
        /// Numer zamówienia z Subiekta<br/>
        /// ZK 21/MAG/03/2023
        /// </summary>
        public string Number
        {
            get => _number;
            set
            {
                if (value == null)
                    _number = null;
                else if (value.Trim().Length > 50)
                    throw new ArgumentException("Order number is too long");
                else
                    _number = value.Trim();
            }
        }

        string _external_number;
        /// <summary>
        /// Identyfikator zamówienia z zewnętrznego systemu: max 30 znaków<br/>
        /// 51424587
        /// </summary>
        [Required]
        public string External_number
        {
            get
            {
                return _external_number;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Order external number is required");

                if (value.Trim().Length > 30)
                    throw new ArgumentException("Order external number is too long (max 30 characters)");

                _external_number = value.Trim();
            }
        }

        string _source;
        /// <summary>
        /// Źródło zamówienia: max 30 znaków<br/>
        /// BL, BL|Dropshipping, BL|Matkam, BL|Kupmeble, MKS, Wayfair, BRW
        /// </summary>
        [Required]
        public string Source
        {
            get => _source;
            set
            {
                if (value == null || value.Trim().Length <= 0)
                    throw new ArgumentException("Order source field is required");

                if (value.Trim().Length > 30)
                    throw new ArgumentException("Order source field is too long (Max. 30 characters)");

                if (value.Contains("."))
                    throw new ArgumentException("Incorrect source. Dot '.' character is forbidden");

                _source = value.Trim();
            }
        }

        string _subtitle = "";
        /// <summary>
        /// Podtytuł zamówienia: do 50 znaków.
        /// </summary>
        public string Subtitle
        {
            get
            {
                return _subtitle;
            }
            set
            {
                if (value == null)
                {
                    _subtitle = null;
                }
                else if (value.Trim().Length > 50)
                {
                    throw new ArgumentException("Subtitle length exceed (max 50 characters)");
                }
                else
                {
                    _subtitle = value.Trim();
                }
            }
        }

        DateTime _created_at;
        /// <summary>
        /// Data złożenia zamówienia
        /// </summary>
        public DateTime? Created_at
        {
            get
            {
                if (_created_at == DateTime.MinValue)
                    return null;

                return _created_at;
            }
            set
            {
                if (value < DateTime.Now.AddYears(-1))
                    throw new ArgumentException("Cannot add order older than 1 year");

                if (value > DateTime.Now.AddYears(1))
                    throw new ArgumentException("Cannot add order newer than 1 year");

                _created_at = value.Value;
            }
        }

        DateTime _deadline;
        /// <summary>
        /// Termin realizacji zamówienia
        /// </summary>
        public DateTime? Deadline
        {
            get
            {
                if (_deadline == DateTime.MinValue)
                    return null;

                return _deadline;
            }

            set
            {
                if (value == null)
                {
                    _deadline = value.Value;
                }
                else if (value < DateTime.Now.AddYears(-1))
                {
                    throw new ArgumentException("Cannot add order with less than 1 year deadline");
                }
                else if (value > DateTime.Now.AddYears(1))
                {
                    throw new ArgumentException("Cannot add order with more then 1 year deadline");
                }
                else
                {
                    _deadline = value.Value;
                }
            }
        }

        /// <summary>
        /// Informacje o kliencie - Login, Email, Telefon
        /// </summary>
        public OrderBuyer Buyer { get; set; }

        /// <summary>
        /// Dostawa zamówienia: Sposób dostawy, Koszt, Adres
        /// </summary>
        public OrderDelivery Delivery { get; set; }

        /// <summary>
        /// Informacje o płatności. Waluta, Zapłacona kwota, Sposób płatności
        /// </summary>
        public OrderPayment Payment { get; set; }

        /// <summary>
        /// Dane do faktury
        /// </summary>
        public OrderInvoice Invoice { get; set; }

        /// <summary>
        /// Produkty w zamówieniu
        /// </summary>
        public List<OrderProduct> Products { get; set; }

        /// <summary>
        /// Dodatkowe dokumenty do wydruku z zamówieniem, np. Awiazacja lub protokół wydania
        /// </summary>
        public List<OrderAdditionalDocument> Additional_documents { get; set; }

        int _categoryId = 1;
        /// <summary>
        /// Id kategorii zamówienia (zamówienie od klienta, duzy hurt, zamówienie na salon, ...)
        /// </summary>
        public int Category_id
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
            }
        }

        public override string ToString()
        {
            string output = "Order(";

            if (Source != null)
            {
                output += $"Source={Source}, ";
            }

            if (External_number != null)
            {
                output += $"External_number={External_number}, ";
            }

            if (Number != null)
            {
                output += $"Number={Number}, ";
            }

            if (Products != null && Products.Count > 0)
            {
                output += $"Products={Products.Count}, ";
            }

            output += ")";
            return output;
        }
    }
}
