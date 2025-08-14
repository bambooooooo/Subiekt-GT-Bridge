namespace SGT_BRIDGE.Models.Order
{
    /// <summary>
    /// Rodzaj podatku VAT
    /// </summary>
    public enum TAX_TYPE
    {
        /// <summary>
        /// (1) Podatek VAT 22%
        /// </summary>
        VAT_22 = 1,
        /// <summary>
        /// (2) Podatek VAT 7% 
        /// </summary>
        VAT_7 = 2,
        /// <summary>
        /// (3) Podatek VAT 3%
        /// </summary>
        VAT_3 = 3,
        /// <summary>
        /// (4) Podatek VAT 0%
        /// </summary>
        VAT_0 = 4,
        /// <summary>
        /// (5) Zwolnienie z podatku VAT
        /// </summary>
        VAT_ZW = 5,
        /// <summary>
        /// (6) Podatek VAT eksportowy 0%
        /// </summary>
        VAT_EX = 6,
        /// <summary>
        /// (7) Podatek VAT UE 0%
        /// </summary>
        VAT_UE = 7,
        /// <summary>
        /// (8) Podatek VAT 5%
        /// </summary>
        VAT_5 = 8,
        /// <summary>
        /// (9) Podatek niedpodlegający odliczeniu
        /// </summary>
        VAT_NPO = 9,
        /// <summary>
        /// (10) Podatek VAT 6%
        /// </summary>
        VAT_6 = 10,
        /// <summary>
        /// (100_001) Podstawowy podatek VAT 23%
        /// </summary>
        VAT_23 = 100_001,
        /// <summary>
        /// (100_002) Podatek VAT 8%
        /// </summary>
        VAT_8 = 100_002,
        /// <summary>
        /// (100_003) Podatek odwrotne obciążenie
        /// </summary>
        VAT_OO = 100_003,
        /// <summary>
        /// (100_004) Techniczne zero do fiskalizacji faktur marża
        /// </summary>
        VAT_TZM = 100_004
    }
}
