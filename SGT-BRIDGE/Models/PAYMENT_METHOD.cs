namespace SGT_BRIDGE.Models
{
    public enum PAYMENT_METHOD
    { 
        /// <summary>
        /// Online payment like Online Transfer, BLIK, VISA, PayPal, etc.
        /// </summary>
        ONLINE = 1,
        /// <summary>
        /// Cash On Delivery Payment
        /// </summary>
        COD = 2,
        /// <summary>
        /// Credit (delayed) payment
        /// </summary>
        CREDIT = 3
    }
}
