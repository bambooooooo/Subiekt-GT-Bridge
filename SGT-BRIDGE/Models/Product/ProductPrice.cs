namespace SGT_BRIDGE.Models.Product
{
    public class ProductPrice
    {
        /// <summary>
        /// Price level system id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Price level code, e.g. Wholesale, Retail
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Price in level's currency
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Is a brutto price?
        /// </summary>
        public bool Brutto { get; set; }
    }
}
