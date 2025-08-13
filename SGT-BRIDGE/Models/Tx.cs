using Microsoft.EntityFrameworkCore;

namespace SGT_BRIDGE.Models
{
    public class Tx
    {
        /// <summary>
        /// Database Primary Key
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Source of order: ecommerce, manual, externalname
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Sku to translate from
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// Translated Sku
        /// </summary>
        public string To { get; set; }
    }
}
