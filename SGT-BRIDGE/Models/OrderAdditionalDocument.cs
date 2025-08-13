namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Dodatkowy dokument do zamówienia, np. awizacja kurierska
    /// </summary>
    public class OrderAdditionalDocument
    {
        /// <summary>
        /// Nazwa dokumentu
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Id kategorii dokumentu
        /// </summary>
        public int CategoryId { get; private set; }
        /// <summary>
        /// Opis dokumentu
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Zawartość dokumentu PDF (base64)
        /// </summary>
        public string Content { get; private set; }
        public OrderAdditionalDocument(string name, string desc, string base64content, int cat_id)
        {
            Name = name;
            Description = desc;
            CategoryId = cat_id;
            Content = base64content;
        }
    }
}
