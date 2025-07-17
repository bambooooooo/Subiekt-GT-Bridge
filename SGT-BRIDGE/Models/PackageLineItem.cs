using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models
{
    public class PackageLineItem
    {
        /// <summary>
        /// System package identifier
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Quantity of package
        /// </summary>
        [Required]
        public decimal Quantity { get; set; }
    }
}