namespace SGT_BRIDGE.Models.Order
{
    public class ShipmentLabel
    {
        /// <summary>
        /// Numer referencyjny klienta
        /// </summary>
        public string Reference_number { get; private set; }

        /// <summary>
        /// Etykieta zakodowana do base64
        /// </summary>
        public string Label { get; private set; }

        public ShipmentLabel(string refNumber, string labelBase64)
        {
            Reference_number = refNumber;
            Label = labelBase64;
        }
    }
}
