using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models.Order
{
    /// <summary>
    /// Faktura do zrealizowanych zamówień
    /// </summary>
    public class InvoiceFile
    {
        [Required]
        /// <summary>
        /// Źródło zamówień, do którego dodano fakturę / paragon
        /// </summary>
        public List<InvoiceFileSourceDocument> SourceDocuments { get; set; }

        [Required]
        /// <summary>
        /// Identyfikator faktury w systemie
        /// </summary>
        public int InvoiceId { get; set; }

        [Required]
        /// <summary>
        /// Numer faktury z systemu
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Podtytuł z faktury
        /// </summary>
        public string Subtitle { get; set; }

        [Required]
        /// <summary>
        /// Identyfikator klienta
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// E-mail klienta, dla którego została wystawiona faktura
        /// </summary>
        public string ClientEmail { get; set; }

        [Required]
        /// <summary>
        /// Kod klienta
        /// </summary>
        public string ClientCode { get; set; }

        [Required]
        /// <summary>
        /// Pełna nazwa klienta
        /// </summary>
        public string ClientName { get; set; }

        [Required]
        /// <summary>
        /// Zakodowana (base64) zawartość pliku PDF z fakturą
        /// </summary>
        public string Base64Content { get; set; }
    }
}
