using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models
{
    public class Product
    {
        /// <summary>
        /// Product system identifier (Pimcore object id) - 23175
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Product system key (Pimcore object key) - STELLA-01-O
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// Name in PL - Szafka pod umywalkę STELLA 80cm...
        /// </summary>
        public string? NamePl { get; set; }
        /// <summary>
        /// Name in EN - Cabinet for washbasin STELLA 80cm...
        /// </summary>
        public string? NameEn { get; set; }
        /// <summary>
        /// Image - base64 encoded (.png or .jpg)
        /// </summary>
        public string? Image { get; set; }
        /// <summary>
        /// Product short description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Ean (GTIN) code
        /// </summary>
        public string? Ean { get; set; }
        /// <summary>
        /// Product weight
        /// </summary>
        public decimal Mass { get; set; }
        /// <summary>
        /// Width in mm
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// Height in mm
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// Depth (Length) in mm
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// Volume in m3
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// List of packages with quantity
        /// </summary>
        public List<PackageLineItem>? Packages { get; set; }
    }
}
