using System.ComponentModel.DataAnnotations;

namespace SGT_BRIDGE.Models.Order
{
    /// <summary>
    /// Potwierdzenie zamówienia - numer systemowy i data dodania
    /// </summary>
    public class OrderConfirmation
    {
        /// <summary>
        /// Źródło zamówienia
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Numer
        /// </summary>
        [Required]
        public string External_number { get; set; }

        /// <summary>
        /// Numer systemowy
        /// </summary>
        [Required]
        public string Number { get; set; }

        /// <summary>
        /// Podtytuł zamówienia
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Data potwierdzenia w systemie
        /// </summary>
        [Required]
        public DateTime Date_add { get; set; }

        /// <summary>
        /// Przybliżona data realizacji
        /// </summary>
        public DateTime Date_est_send { get; set; }

        public override string ToString()
        {
            return $"OrderConfirmation(Source={Source}, ExternalNumber={External_number}, Number={Number}, Date={Date_add})";
        }
    }
}
