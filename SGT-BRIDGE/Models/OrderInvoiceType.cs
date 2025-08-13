namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Typ danych na fakturze - czy na osobę prywatną, czy na firmę
    /// </summary>
    public enum OrderInvoiceType
    {
        /// <summary>
        /// Klient detaliczny (własny klient z marketplace)
        /// </summary>
        CLIENT = 1,
        /// <summary>
        /// Klient detaliczny, który posiada NIP (własny klient z marketplace)
        /// </summary>
        CLIENT_WITH_NIP = 2,
        /// <summary>
        /// Firma (własny klient z marketplace)
        /// </summary>
        COMPANY = 3,
        /// <summary>
        /// Kontrahent (zakłada się, że jego NIP jest już w bazie)
        /// </summary>
        RECEIVER = 4
    }
}
