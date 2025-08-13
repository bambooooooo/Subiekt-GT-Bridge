namespace SGT_BRIDGE.Endpoints
{
    public static partial class OrderEndpoint
    {
        public class OrderItemPrice
        {
            /// <summary>
            /// Nazwa (źródło) ceny
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Cena pozycji brutto
            /// </summary>
            public decimal Brutto { get; set; }
            /// <summary>
            /// Rabat w procentach
            /// </summary>
            public decimal Disocunt { get; set; }

            public override string ToString()
            {
                return $"{Brutto:0.00} ({Disocunt:0}%)";
            }
        }
    }
}
