namespace SGT_BRIDGE.Endpoints
{
    public static partial class OrderEndpoint
    {
        public class OrderItem
        {
            /// <summary>
            /// Id pozycji (ob_Id)
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Obecna ilość j.m. pozycji
            /// </summary>
            public decimal CurrentCnt { get; set; }
            /// <summary>
            /// Ilość j.m.
            /// </summary>
            public decimal Cnt { get; set; }
            /// <summary>
            /// Kod produktu
            /// </summary>
            public string Code { get; set; }
            /// <summary>
            /// Kod kreskowy produktu
            /// </summary>
            public string Ean { get; set; }
            /// <summary>
            /// Cena z zamówienia. Jeśli pusta - przeliczana wg schematu
            /// </summary>
            public decimal? OrderPrice { get; set; }
            public OrderItemPrice PriceOrder { get; set; }
            /// <summary>
            /// Obecna cena pozycji
            /// </summary>
            public decimal CurrentPrice { get; set; }
            public OrderItemPrice PriceCurrent { get; set; }
            /// <summary>
            /// Domyślna cena towaru dla obecnego poziomu ceny
            /// </summary>
            public decimal PriceProduct { get; set; }
            public OrderItemPrice PriceProductPriceLevel { get; set; }
            /// <summary>
            /// Cena pozycji po domyślnym rabacie dla klienta
            /// </summary>
            public decimal PriceClientDefaultDrop { get; set; }
            public OrderItemPrice PriceClientDrop { get; set; }

            /// <summary>
            /// Cena pozycji po domyślnym rabacie dla klienta
            /// </summary>
            public decimal PriceProductDefaultDrop { get; set; }
            public OrderItemPrice PriceProductDrop { get; set; }

            public decimal PricePromotion { get; set; }
            public OrderItemPrice PriceAvailablePromotion { get; set; }
            public OrderItemPrice PriceFinal
            {
                get
                {
                    if (PriceOrder != null)
                        return PriceOrder;

                    List<OrderItemPrice> prices = new List<OrderItemPrice>();

                    if (PriceCurrent.Brutto > 0)
                    {
                        prices.Add(PriceCurrent);
                    }


                    if (PriceProductDrop.Brutto > 0)
                    {
                        prices.Add(PriceProductDrop);
                    }

                    if (PriceClientDrop.Brutto > 0)
                    {
                        prices.Add(PriceClientDrop);
                    }

                    if (PriceAvailablePromotion.Brutto > 0)
                    {
                        prices.Add(PriceAvailablePromotion);
                    }

                    if (prices.Count == 0)
                    {
                        Console.WriteLine($"Product(Code={Code}) has no correct prices correlacted with actual pricelevel");
                        Console.WriteLine("Please fill Product's correct prices and set correct price levels");
                        return new OrderItemPrice() { Name = "Brak ceny", Brutto = 0, Disocunt = 0 };
                    }

                    return prices.OrderBy(x => x.Brutto).First();
                }
            }
        }
    }
}
