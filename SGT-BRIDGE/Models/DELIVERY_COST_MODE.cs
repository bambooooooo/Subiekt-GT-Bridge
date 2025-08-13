namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Określa w jaki sposób przetwarzać koszty transportu
    /// </summary>
    public enum DELIVERY_COST_MODE
    {
        /// <summary>
        /// Pomiń
        /// </summary>
        SKIP = 0,
        /// <summary>
        /// Dodaj jako usługa TRANSPORT.<br/>
        /// Uwaga! Usługa musi być wcześniej dodana do bazy Subiekta
        /// </summary>
        ADD = 1,
        
    }
    
}
