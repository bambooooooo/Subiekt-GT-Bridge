namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Rodzaj operacji VAT
    /// </summary>
    public enum VAT_OPERATION_TYPE
    {
        /// <summary>
        /// S
        /// </summary>
        NABYCIE_DOSTAWA_KRAJOWA = 0,
        /// <summary>
        /// WDT
        /// </summary>
        IMPORT_EKSPORT_TOWAROW = 1,
        /// <summary>
        /// UE
        /// </summary>
        NABYCIE_DOSTAWA_UE = 2,
        /// <summary>
        /// WTTD
        /// </summary>
        TRANSAKCJA_TROJSTRONNA = 3,
        /// <summary>
        /// EXU
        /// </summary>
        IMPORT_EKSPORT_USLUG = 4,
        /// <summary>
        /// OOs
        /// </summary>
        ODWROTNE_OBCIAZENIE_SPRZEDAZ = 6,
        /// <summary>
        /// EX
        /// </summary>
        NABYCIE_DOSTAWA_POZA_UE = 12,
        /// <summary>
        /// OOu
        /// </summary>
        ODWROTNE_OBCIAZENIE_SWIADCZENIE_USLUG = 21
    }
}
