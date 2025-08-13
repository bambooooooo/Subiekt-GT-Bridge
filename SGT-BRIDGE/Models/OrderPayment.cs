using SGT_BRIDGE.Utils;

namespace SGT_BRIDGE.Models
{
    public partial class OrderPayment
    {
        PAYMENT_METHOD _method = PAYMENT_METHOD.CREDIT;
        /// <summary>
        /// Metoda płatności: ONLINE (Przelew), COD (Za pobraniem), CREDIT (Kredyt kupiecki)
        /// </summary>
        public PAYMENT_METHOD Method
        {
            get => _method;
            set
            {
                if ((int)value < 1 || (int)value > 3)
                    throw new ArgumentException($"Incorrect payment method={value}");

                _method = value;
            }
        }

        decimal _amount_done = 0.0m;
        /// <summary>
        /// Zapłacona kwota
        /// </summary>
        public decimal Amount_done
        {
            get => _amount_done;
            set
            {
                if (value < 0)
                    throw new ArgumentException($"Order payment='{value}' can not be lower than zero");

                _amount_done = value;
            }
        }

        string _currency = null;
        /// <summary>
        /// Waluta: PLN, USD, EUR (3 znaki iso)<br/>
        /// <see cref="https://en.wikipedia.org/wiki/ISO_4217"/>
        /// </summary>
        public string Currency
        {
            get => _currency;
            set
            {
                if (!(value is null))
                {
                    if (!Utils.Utils.CURRENCY_ISO.Contains(value))
                        throw new ArgumentException($"Unknown currency='{value}'");
                }

                _currency = value;
            }
        }

        public OrderPayment() { }
    }
}
