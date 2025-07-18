namespace SGT_BRIDGE.Models
{
    public class ProductPrice
    {
        /// <summary>
        /// Price level code, e.g. Wholesale, Retail
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Price in level's currency
        /// </summary>
        public decimal Price { get; set; }
    }
}
