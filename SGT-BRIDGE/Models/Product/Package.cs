using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models.Product
{
    public class Package
    {
        /// <summary>
        /// System Id (Pimcore object id)
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// System key(Pimcore object key)
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// Package weight
        /// </summary>
        public decimal Mass { get; set; }
        /// <summary>
        /// Package length in mm
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// Package width in mm
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// Package height in mm
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// Package barcode
        /// </summary>
        public string? Barcode { get; set; }
        /// <summary>
        /// Package volume
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Base price (TKW)
        /// </summary>
        public decimal BasePrice { get; set; }
    }
}