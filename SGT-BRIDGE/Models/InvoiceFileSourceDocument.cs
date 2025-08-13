using System;
using System.Collections.Generic;

namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Dokument źródłowy, na podstawie którego powstała faktura (może być ich więcej niż 1)
    /// </summary>
    public class InvoiceFileSourceDocument
    {
        /// <summary>
        /// Id dokumentu w systemie
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Numer dokumentu w systemie
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Numer zewnętrzny dokumentu
        /// </summary>
        public string ExternalNumber { get; set; }
        /// <summary>
        /// Podtytuł dokumentu
        /// </summary>
        public string Subtitle { get; set; }
        /// <summary>
        /// Data dodania dokumentu
        /// </summary>
        public DateTime DateAdd { get; set; }
        /// <summary>
        /// Źródło dokumentu (dotyczy ZK)
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Pozycje na dokumencie
        /// </summary>
        public List<InvoiceFileSourceDocumentItem> Items { get; set; }
        /// <summary>
        /// Powiązany dokument źródłowy (np. ZK do WZ)
        /// </summary>
        public InvoiceFileSourceDocument SourceDocument { get; set; }
    }
}
