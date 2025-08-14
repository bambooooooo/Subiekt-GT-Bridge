namespace SGT_BRIDGE.Models.Order
{
    public class OrderDelivery
    {
        /// <summary>
        /// Adres dostawy
        /// </summary>
        public Address Address { get; set; }

        string _code;
        /// <summary>
        /// Kod Dostawy (zazwczaj kod magazynu / salonu)
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
            }
        }

        string _delivery_metod = "Inny";
        /// <summary>
        /// Metoda dostawy. Domyślnie: Inny<br/>
        /// Przykłady:<br/>
        /// DPD<br/>
        /// Zadbano<br/>
        /// GEIS<br/>
        /// Domyślnie: Inna
        /// </summary>
        public string Method
        {
            get => _delivery_metod;
            set
            {
                _delivery_metod = value;
                if (value == null || value == "")
                    _delivery_metod = "Inna";
            }
        }

        decimal _price = 0m;
        /// <summary>
        /// Koszt dostawy (brutto). Domyślnie 0,00zł
        /// </summary>
        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Delivery price cannot be lower than zero.");

                _price = value;
            }
        }

        DELIVERY_COST_MODE _delivery_cost_mode = DELIVERY_COST_MODE.SKIP;

        /// <summary>
        /// Sposób przetwarzania kosztów dostawy. Domyślnie: Pomiń
        /// </summary>
        public DELIVERY_COST_MODE Cost_mode
        {
            get
            {
                return _delivery_cost_mode;
            }
            set
            {
                _delivery_cost_mode = value;
            }
        }

        public OrderDelivery() { }

        public OrderDelivery(string code)
        {
            Code = code;
        }
    }
}
