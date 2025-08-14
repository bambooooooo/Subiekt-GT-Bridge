using InsERT;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGT_BRIDGE.Models;
using SGT_BRIDGE.Models.Order;
using SGT_BRIDGE.Services;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace SGT_BRIDGE.Endpoints
{
    public static partial class OrderEndpoint
    {
        public static void RegisterOrderEndpoint(this WebApplication app)
        {
            var items = app.MapGroup("/orders");

            items.MapGet("/{id:int}", Get);
            items.MapGet("/{source}/{externalNumber}", GetExternal);
            items.MapPost("/", Post);
            items.MapPut("/realize/{id:int}", Realize);
            items.MapPut("/realize/{source}/{externalNumber}", RealizeExternal);
            items.MapGet("/realize/{source}/{externalNumber}", RealizeExternal);

            items.MapDelete("/{id}", Delete);
            items.MapDelete("/{source}/{externalNumber}", DeleteExternal);
            items.MapGet("/tx", GetTranslations);
            items.MapGet("/tx/{source}/{from}", GetTranslation);
            items.MapPost("/tx/{source}/{from}", UpdateTranslation);
            items.MapDelete("/tx/{source}/{from}", DeleteTranslation);
            items.MapDelete("/tx/{id:int}", DeleteTranslationById);

            items.MapPut("/flag/{id}", SetFlagById);

            items.MapGet("/test", (SubiektGT worker) =>
            {
                return worker.EnqueueAsync<IResult>(subiekt =>
                {
                    var zks = new List<SuDokument>();

                    SuDokument zkKowalski = subiekt.SuDokumentyManager.DodajZK();
                    zkKowalski.KontrahentId = 122;
                    zks.Add(zkKowalski);

                    SuDokument zkKupmeble = subiekt.SuDokumentyManager.DodajZK();
                    zkKupmeble.KontrahentId = 1034;
                    zks.Add(zkKupmeble);

                    SuDokument zkAgata = subiekt.SuDokumentyManager.DodajZK();
                    zkAgata.KontrahentId = 612;
                    zks.Add(zkAgata);

                    SuDokument zkFurnlux = subiekt.SuDokumentyManager.DodajZK();
                    zkFurnlux.KontrahentId = 1287;
                    zks.Add(zkFurnlux);

                    SuDokument zkNapkins = subiekt.SuDokumentyManager.DodajZK();
                    zkNapkins.KontrahentId = 4195;
                    zks.Add(zkNapkins);

                    foreach (SuDokument zk in zks)
                    {
                        zk.DataOtrzymania = DateTime.Now;
                        zk.Podtytul = "API";

                        zk.PobierzParametryKontrahenta();

                        Console.WriteLine($"[{zk.KontrahentId}] Waluta: {zk.WalutaSymbol}");

                        if (zk.WalutaSymbol != "PLN")
                        {
                            zk.PobierzKursWalutyWgParametrow();
                        }

                        SuPozycja poz = zk.Pozycje.Dodaj(7701);
                        poz.IloscJm = (decimal)1;

                        zk.Zapisz();
                    }

                    return TypedResults.Ok("Ok.");
                });
            });
            items.MapGet("/test/fixed", (SubiektGT worker) =>
            {
                return worker.EnqueueAsync<IResult>(subiekt =>
                {
                    SuDokument zk = subiekt.SuDokumentyManager.DodajZK();
                    zk.KontrahentId = 122;
                    zk.DataOtrzymania = DateTime.Now;
                    zk.Podtytul = "API - cena z baselinkera";

                    SuPozycja poz = zk.Pozycje.Dodaj(7701);
                    poz.IloscJm = (decimal)1;
                    poz.CenaBruttoPrzedRabatem = (decimal)188.88;
                    poz.CenaBruttoPoRabacie = (decimal)188.88;

                    zk.Zapisz();

                    return TypedResults.Ok("Ok 2 ");
                });
            });
        }
        /// <summary>
        /// Retrieves order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> Get(int id, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                if (!subiekt.SuDokumentyManager.Istnieje(id))
                {
                    return TypedResults.NotFound("Order not found");
                }

                return GetOrder(id, worker, subiekt);
            });
        }

        private static IResult GetOrder(int id, SubiektGT worker, Subiekt subiekt)
        {
            SuDokument doc = subiekt.SuDokumentyManager.Wczytaj(id);
            if (doc.Typ != (int)InsERT.SuDokumentTypEnum.gtaSuDokumentTypZK)
            {
                return TypedResults.BadRequest("Document with this id is not an order");
            }

            Order o = new();

            o.Source = doc.PoleWlasne[worker.SOURCE_FIELD_NAME] is DBNull ? "SYSTEM" : doc.PoleWlasne[worker.SOURCE_FIELD_NAME];
            o.Number = doc.NumerPelny;
            o.External_number = doc.NumerOryginalny is DBNull ? string.Empty : doc.NumerOryginalny;
            o.Created_at = doc.DataWystawienia;
            o.Deadline = doc.TerminRealizacji;
            o.Subtitle = doc.Podtytul is DBNull ? string.Empty : doc.Podtytul;

            o.Products = new();
            foreach (SuPozycja li in doc.Pozycje)
            {
                o.Products.Add(new OrderProduct()
                {
                    Position = (int)li.Lp,
                    Code = li.TowarSymbol,
                    Name = li.TowarNazwa,
                    Quantity = (int)li.IloscJm,
                    Price = li.CenaBruttoPoRabacie,
                    Price_drop = li.RabatProcent,
                    Descrption = li.Opis is DBNull ? string.Empty : li.Opis
                });
            }


            Kontrahent client = subiekt.KontrahenciManager.Wczytaj(doc.KontrahentId);
            //KhAdresDostawy deliveryAddress = client.AdresyDostaw.Wczytaj(doc.AdresDostawyId);

            //o.Invoice = new()
            //{
            //    NIP = client.NIP,
                //Firstname = client.OsobaImie,
                //Lastname = client.OsobaNazwisko,
                //Company = client.NazwaPelna,
                //Address = new()
                //{
                //    Street = client.AdrDostUlica,
                //    City = client.AdrDostMiejscowosc,
                //    Postcode = client.AdrDostKodPocztowy,
                //}
            //};

            //o.Buyer = new()
            //{

            //}

            //o.Payment = new()
            //{

            //};

            //o.Delivery = new()
            //{

            //}

            return TypedResults.Ok(o);
        }

        /// <summary>
        /// Retrieves order
        /// </summary>
        /// <param name="source"></param>
        /// <param name="externalNumber"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> GetExternal(string source, string externalNumber, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                var predicate = BuildFilter("pwd_Tekst02", source, externalNumber);
                var res = worker.db.vwPolaWlasne_Dokument.FirstOrDefault(predicate);

                if (res == default)
                {
                    return TypedResults.NotFound();
                }

                return GetOrder(res.dok_Id, worker, subiekt);
            });
        }

        private static Expression<Func<vwPolaWlasne_Dokument, bool>> BuildFilter(string fieldName, string source, string externalNumber)
        {
            var param = Expression.Parameter(typeof(vwPolaWlasne_Dokument), "x");

            // x.dok_NrPelnyOryg == externalNumber
            var leftExternal = Expression.Property(param, nameof(vwPolaWlasne_Dokument.dok_NrPelnyOryg));
            var rightExternal = Expression.Constant(externalNumber);
            var equalsExternal = Expression.Equal(leftExternal, rightExternal);

            // x.pwd_Tekst0X == source
            var leftDynamic = Expression.Property(param, fieldName);
            var rightDynamic = Expression.Constant(source);
            var equalsDynamic = Expression.Equal(leftDynamic, rightDynamic);

            var and = Expression.AndAlso(equalsExternal, equalsDynamic);

            return Expression.Lambda<Func<vwPolaWlasne_Dokument, bool>>(and, param);
        }

        /// <summary>
        /// Add or update order
        /// </summary>
        /// <param name="o"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> Post(Order o, SubiektGT worker, TxContext txContext)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                InsERT.SuDokument order = null;
                InsERT.Kontrahent client = null;

                UpdateResult result = new();

                if (!Exist(worker, o.Source, o.External_number))
                {
                    order = subiekt.SuDokumentyManager.DodajZK();

                    order.Tytul = "Zamówienie od klienta";
                    order.NumerOryginalny = o.External_number;
                    order.PoleWlasne["Źródło"] = o.Source;
                    order.StatusDokumentu = 7;
                    order.Rezerwacja = true;
                    order.LiczonyOdCenNetto = true;
                    order.KategoriaId = o.Category_id;

                    if (o.Subtitle != null)
                    {
                        order.Podtytul = o.Subtitle;
                    }

                    if (o.Invoice == null)
                    {
                        return TypedResults.BadRequest("Invoice data must exist during order creation.");
                    }

                    order.KontrahentId = GetClient(subiekt, worker, o.Invoice);
                    order.Zapisz();

                    Console.WriteLine($"Order(Source={o.Source}, ExtNumber={o.External_number}, No={order.NumerPelny}) created.");

                    result.Confimation = new OrderConfirmation()
                    {
                        Source = o.Source,
                        External_number = o.External_number,
                        Date_add = DateTime.Now,
                        Number = order.NumerPelny,
                        Subtitle = order.Podtytul,
                        Date_est_send = order.TerminRealizacji
                    };

                    order.Zamknij();
                }

                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        order = subiekt.SuDokumentyManager.Wczytaj(GetId(worker, o.Source, o.External_number));
                        order.Tytul = order.Tytul; // LOCK

                        UpdateOrderSubtitle(ref order, o, result);
                        UpdateOrderCreateTime(ref order, o, result);
                        UpdateOrderDeadline(ref order, o, result);
                        UpdateOrderBuyerComment(ref order, o, result);
                        UpdateOrderDeliveryMethod(worker, ref order, o, result);
                        UpdateOrderDeliveryAddress(subiekt, ref order, o, result);
                        UpdateOrderInvoiceAddress(subiekt, ref order, o, result);

                        client = subiekt.KontrahenciManager.Wczytaj(order.KontrahentId);

                        UpdateBuyerEmail(subiekt, ref client, o, result);
                        UpdateBuyerLogin(subiekt, ref client, o, result);
                        UpdateBuyerPhone(subiekt, ref client, o, result);

                        UpdateOrderCurrency(ref order, ref client, o, result);
                        UpdateOrderVatOperation(ref order, ref client, o, result);
                        UpdateOrderPriceLevel(ref order, ref client, o, result);
                        UpdateOrderCurrencyRate(ref order);

                        UpdateOrderPositions(worker, subiekt, txContext, ref order, o, result);
                        UpdateOrderPayment(worker, ref order, ref client, o, result);

                        Console.WriteLine($"Order value: {order.WartoscBrutto} {order.WalutaSymbol}");

                        order.Zapisz();
                        Console.WriteLine("Order saved");
                        break;
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        if (ex.Message.Contains("został zablokowany przez operatora"))
                        {
                            Console.WriteLine("Object is locked. Retrying in 10s...");
                            Thread.Sleep(10_000);
                        }
                        else
                        {
                            TypedResults.BadRequest(ex);
                            break;
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);

                        TypedResults.BadRequest(ex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);

                        TypedResults.BadRequest(ex);
                        break;
                    }
                    finally
                    {
                        if (order != null)
                            order.Zamknij();

                        if (client != null)
                            client.Zamknij();
                    }
                }

                return TypedResults.Ok();
            });
        }

        private static int GetClient(Subiekt subiekt, SubiektGT worker, OrderInvoice invoice)
        {
            bool found = false;

            if (invoice.NIP != null && invoice.NIP != "")
            {
                found = subiekt.KontrahenciManager.IstniejeWg(invoice.NIP, InsERT.KontrahentParamWyszukEnum.gtaKontrahentWgNip);
            }

            if (invoice.NIP != null && invoice.NIP != "" && found)
            {
                InsERT.Kontrahent kh = subiekt.KontrahenciManager.WczytajKontrahentaWg(invoice.NIP, InsERT.KontrahentParamWyszukEnum.gtaKontrahentWgNip);
                int id = kh.Identyfikator;
                kh.Zamknij();

                return id;
            }
            else if (invoice.NIP != null && invoice.NIP != "" && !found && invoice.InvoiceType == OrderInvoiceType.RECEIVER)
            {
                throw new ArgumentException($"Klient z NIP={invoice.NIP} nie istnieje w bazie");
            }
            else
            {
                InsERT.Kontrahent client = subiekt.KontrahenciManager.DodajKontrahenta();

                client.Typ = InsERT.KontrahentTypEnum.gtaKontrahentTypOdbiorca;
                client.Symbol = GetNextClientId(worker);

                switch (invoice.InvoiceType)
                {
                    case OrderInvoiceType.CLIENT:
                        {
                            client.Osoba = true;
                            client.OsobaImie = invoice.Firstname;
                            client.OsobaNazwisko = invoice.Lastname;
                            client.OdbiorcaDetaliczny = true;
                            client.WWW = true;

                            break;
                        }
                    case OrderInvoiceType.CLIENT_WITH_NIP:
                        {
                            client.NIP = invoice.NIP;
                            client.Osoba = true;
                            client.OsobaImie = invoice.Firstname;
                            client.OsobaNazwisko = invoice.Lastname;
                            client.OdbiorcaDetaliczny = true;
                            client.WWW = true;

                            break;
                        }
                    case OrderInvoiceType.COMPANY:
                        {
                            client.Osoba = false;
                            client.NIP = invoice.NIP;
                            client.Nazwa = invoice.Company_short;
                            client.NazwaPelna = invoice.Company;
                            client.WWW = true;
                            client.OdbiorcaDetaliczny = true;

                            break;
                        }
                }

                client.Zapisz();
                int id = client.Identyfikator;
                client.Zamknij();

                return id;
            }
        }

        static string FixString(string inputstring)
        {
            return inputstring.Replace("¿", "ż").Replace("³", "ł");
        }

        /// <summary>
        /// Aktualizuje płatność w zamówieniu
        /// </summary>
        /// <param name="order"></param>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        /// <param name="settings"></param>
        private static void UpdateOrderPayment(SubiektGT worker, ref SuDokument order, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (o.Payment != null)
            {
                if (!client.OdbiorcaDetaliczny)
                {
                    // Znany klient -> Zawsze KREDYT_KUPIECKI
                    // Termin kredytu powinien być uwzględniony w parametrach kontrahenta
                    if (client.KredytKupieckiTerminId is DBNull)
                    {
                        order.PlatnoscKredytKwota = order.WartoscBrutto;
                        result.Warnings.Add($"Known Client({client.Symbol}) has no default CREDIT_PAYMENT_DEADLINE. Plase update. Deadline has been set to 7 days");
                        order.PlatnoscKredytTermin = DateTime.Now.AddDays(7);
                    }
                    else
                    {
                        if (order.PlatnoscKredytId != client.KredytKupieckiTerminId)
                        {
                            result.Changes.Add($"Payment deadline: Id({order.PlatnoscKredytId}) => {client.KredytKupieckiTerminId}");
                            order.PlatnoscKredytId = client.KredytKupieckiTerminId;
                        }
                    }

                    if (order.PlatnoscKredytKwota != order.WartoscBrutto)
                    {
                        result.Changes.Add($"Payment amount: {order.PlatnoscKredytKwota} => {order.WartoscBrutto}");
                        order.PlatnoscKredytKwota = order.WartoscBrutto;
                    }
                }
                else
                {
                    // Klient detaliczny
                    // Forma płatności zależna od formy dostawy: płatność z góry / pobranie
                    // Ostrzeżenie jeśli zaznaczono opcję Kredyt Kupiecki
                    if (o.Payment.Method == PAYMENT_METHOD.ONLINE)
                    {
                        if (order.PlatnoscPrzelewKwota <= 0 && order.PlatnoscPrzelewKwota != o.Payment.Amount_done)
                        {
                            result.Changes.Add($"Payment method: ONLINE_PAYMENT");
                        }

                        if (order.PlatnoscPrzelewKwota != o.Payment.Amount_done)
                        {
                            result.Changes.Add($"Payment amount: {order.PlatnoscPrzelewKwota} => {o.Payment.Amount_done}");
                            order.PlatnoscPrzelewKwota = o.Payment.Amount_done;
                            if (order.WartoscBrutto < o.Payment.Amount_done)
                            {
                                order.PlatnoscPrzelewKwota = order.WartoscBrutto;
                                decimal overPayment = o.Payment.Amount_done - order.WartoscBrutto;
                                result.Warnings.Add($"Transfer payment is too high. Expected={order.WartoscBrutto}, Actual={o.Payment.Amount_done}, Difference={overPayment}.");
                            }

                            if (order.WartoscBrutto - o.Payment.Amount_done > 0)
                            {
                                result.Warnings.Add($"Delayed(Days={7}) To Pay: {(decimal)order.PlatnoscGotowkaKwota:.00}");
                                order.PlatnoscKredytKwota = order.PlatnoscGotowkaKwota;
                                //order.PlatnoscKredytTermin = DateTime.Now.AddDays(7);
                            }
                        }
                    }
                    else if (o.Payment.Method == PAYMENT_METHOD.COD)
                    {
                        if (order.PlatnoscRatyId is DBNull || order.PlatnoscRatyId != worker.SGT_TRANSPORT_SERVICE_ID)
                        {
                            result.Changes.Add($"Payment method: COD_PAYMENT");
                            order.PlatnoscRatyId = worker.SGT_TRANSPORT_SERVICE_ID;
                        }

                        if (order.PlatnoscRatyKwota != order.WartoscBrutto)
                        {
                            result.Changes.Add($"Payment amount: {order.PlatnoscRatyKwota} => {o.Payment.Amount_done}");
                            order.PlatnoscRatyKwota = order.WartoscBrutto;
                        }
                    }
                    else
                    {
                        order.PlatnoscKredytKwota = order.WartoscBrutto;
                        result.Warnings.Add($"Client({client.Symbol}) has no default CREDIT_PAYMENT_DEADLINE. Plase update. Deadline has been set to 7 days");
                        order.PlatnoscKredytTermin = DateTime.Now.AddDays(7);
                    }
                }
            }
        }

        /// <summary>
        /// Aktualizuje rodzaj transakcji VAT dla zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderVatOperation(ref SuDokument order, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (!(client.DomyslnaTransVATSprzedaz == int.MinValue))
            {
                if (client.DomyslnaTransVATSprzedaz != order.TransakcjaRodzajOperacjiVat)
                {
                    result.Changes.Add($"VAT Type: {order.TransakcjaRodzajOperacjiVat} => {client.DomyslnaTransVATSprzedaz}");
                    order.TransakcjaRodzajOperacjiVat = client.DomyslnaTransVATSprzedaz;
                }
            }
        }

        /// <summary>
        /// Aktualizuje poziom ceny dla zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderPriceLevel(ref SuDokument order, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (!(client.PoziomCenPrzySprzedazyId is DBNull))
            {
                if (order.PoziomCenyId != client.PoziomCenPrzySprzedazyId)
                {
                    result.Changes.Add($"Pricelevel: {order.PoziomCenyId} => {client.PoziomCenPrzySprzedazyId}");
                    order.PoziomCenyId = client.PoziomCenPrzySprzedazyId;
                    order.PrzeliczRabatWg(InsERT.TypRabatuEnum.gtaTypRabatuProcent, 0);
                }
            }
        }

        /// <summary>
        /// Aktualizuje walutę na zamówieniu
        /// </summary>
        /// <param name="order"></param>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderCurrency(ref SuDokument order, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (client.DomyslnaWaluta is string)
            {
                if (o.Payment != null && o.Payment.Currency != null)
                {
                    SetOrderCurrency(ref order, o, result);
                    if (order.WalutaSymbol != o.Payment.Currency)
                    {
                        result.Changes.Add($"Currency={order.WalutaSymbol} => {o.Payment.Currency} (From Order)");
                        order.WalutaSymbol = o.Payment.Currency;
                    }
                }
                else
                {
                    if (client.DomyslnaWaluta != order.WalutaSymbol)
                    {
                        result.Changes.Add($"Currency={order.WalutaSymbol} => {client.DomyslnaWaluta} (From Client Deafults)");
                        order.WalutaSymbol = client.DomyslnaWaluta;
                    }
                }
            }
            else
            {
                if (o.Payment != null && o.Payment.Currency != null)
                {
                    if (order.WalutaSymbol != o.Payment.Currency)
                    {
                        result.Changes.Add($"Currency={order.WalutaSymbol} => {o.Payment.Currency} (From Order)");
                        order.WalutaSymbol = o.Payment.Currency;
                    }
                }
                else
                {
                    if (order.WalutaSymbol != "PLN")
                    {
                        result.Changes.Add($"Currency={order.WalutaSymbol} => PLN (Default)");
                        order.WalutaSymbol = "PLN";
                    }
                }
            }
        }

        /// <summary>
        /// Aktualizuje kurs na zamówieniu o walucie innej niż polski złoty
        /// </summary>
        /// <param name="order"></param>
        private static void UpdateOrderCurrencyRate(ref SuDokument order)
        {
            if (order.WalutaSymbol != "PLN")
            {
                order.WalutaTypKursu = InsERT.WalutaRodzajKursuEnum.gtaWalutaKursSredni;
                order.WalutaDataKursu = DateTime.Now;

                order.PobierzKursWalutyWgParametrow();
            }
        }

        private static int GetNextClientId(SubiektGT worker)
        {
            return worker.db.vwPolaWlasne_Kontrahent.Max(x => x.kh_Id) + 1;
        }

        /// <summary>
        /// Aktulizuje pozycje na zamówieniu
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        /// <param name="settings"></param>
        private static void UpdateOrderPositions(SubiektGT worker, Subiekt subiekt, TxContext txes, ref SuDokument order, Order o, UpdateResult result)
        {
            bool orderedPositions;
            bool unorderedPositions;
            decimal currentTransportPrice = 0.0m;
            int transportServiceItemId = 0;

            if (o.Products != null)
            {
                orderedPositions = o.Products.All(x => x.Position != null && x.Position >= 0);
                unorderedPositions = o.Products.All(x => x.Position == null);

                if (!unorderedPositions && !orderedPositions)
                {
                    throw new ArgumentException("Inconsistent order item numbers (fixed and auto occured)");
                }

                // Numerowanie
                if (orderedPositions)
                {
                    // Sprawdzenie poprawności obecnego numerowania pozycji
                    for (int pos = 0; pos < o.Products.Count; pos++)
                    {
                        if (o.Products.Find(x => x.Position == pos + 1) == default(OrderProduct))
                        {
                            throw new ArgumentException($"Cannot find OrderItem with Posiotion={pos}");
                        }
                    }
                }
                else
                {
                    // Brak numerów -> numerowanie pozycji (zaczynając od 1)
                    for (int pos = 0; pos < o.Products.Count; pos++)
                    {
                        o.Products[0].Position = pos + 1;
                    }
                }

                // Usuwanie zbędnych pozycji
                // Pobieranie informacji o kosztach transportu (usługa=TRANSPORT)
                // (Tymczasowe) usuwanie TRANSPORT'u
                int index = 0;
                foreach (InsERT.SuPozycja poz in order.Pozycje)
                {
                    InsERT.Towar tw = subiekt.TowaryManager.Wczytaj(poz.TowarId);

                    if (poz.TowarId == worker.SGT_TRANSPORT_SERVICE_ID)
                    {
                        transportServiceItemId = index;
                        currentTransportPrice = poz.CenaBruttoPoRabacie;
                        poz.Usun();

                        if (o.Delivery != null && (o.Delivery.Price == 0 || o.Delivery.Cost_mode == DELIVERY_COST_MODE.SKIP))
                        {
                            result.Changes.Add($"Item(Code={tw.Symbol}, Ean={tw.KodyKreskowe.Podstawowy}) removed");
                        }

                        continue;
                    }

                    if (poz.Lp < o.Products.Count + 1)
                    {
                        if (!((o.Products[poz.Lp - 1].Code != null && o.Products[poz.Lp - 1].Code.Length > 0 && o.Products[poz.Lp - 1].Code == tw.KodyKreskowe.Podstawowy)
                            || o.Products[poz.Lp - 1].Code == tw.Symbol))
                        {
                            result.Changes.Add($"Item(Code={tw.Symbol}, Ean={tw.KodyKreskowe.Podstawowy}) removed");
                            poz.Usun();
                        }
                    }
                    else
                    {
                        if (poz.TowarId == worker.SGT_TRANSPORT_SERVICE_ID)
                        {
                            currentTransportPrice = poz.CenaBruttoPoRabacie;
                            poz.Usun();

                            if (o.Delivery != null && (o.Delivery.Price == 0 || o.Delivery.Cost_mode == DELIVERY_COST_MODE.SKIP))
                            {
                                result.Changes.Add($"Item(Code={tw.Symbol}, Ean={tw.KodyKreskowe.Podstawowy}) removed");
                            }
                        }
                        else
                        {
                            result.Changes.Add($"Item(Code={tw.Symbol}, Ean={tw.KodyKreskowe.Podstawowy}) removed");
                            poz.Usun();
                        }
                    }

                    tw.Zamknij();
                    index++;
                }

                // Dodawanie i aktualizacja poprawnych ilości
                index = 0;
                foreach (var p in o.Products)
                {
                    Towar tw = null;

                    var tx = txes.TxItems.FirstOrDefault(x => x.Source == o.Source && x.From == p.Code);

                    if (tx != default)
                    {
                        string foundCode = tx.To;

                        if (foundCode != null && foundCode.Length > 0 && subiekt.TowaryManager.IstniejeWg(foundCode, InsERT.TowarParamWyszukEnum.gtaTowarWszystko))
                        {
                            tw = subiekt.TowaryManager.WczytajTowarWg(foundCode, InsERT.TowarParamWyszukEnum.gtaTowarWszystko);
                            result.Warnings.Add($"Product(Code={foundCode}) has been translated (Source={o.Source}, From={p.Code})");
                        }

                        p.Code = foundCode;
                    }
                    else if (p.Code != null && p.Code.Length > 0 && subiekt.TowaryManager.IstniejeWg(p.Code, InsERT.TowarParamWyszukEnum.gtaTowarWszystko))
                    {
                        tw = subiekt.TowaryManager.WczytajTowarWg(p.Code, InsERT.TowarParamWyszukEnum.gtaTowarWszystko);

                        if (tw.Aktywny == false)
                        {
                            Console.WriteLine($"Product(Code={p.Code}) is inactive. Looking for translation...");

                            if (tx != default)
                            {
                                string foundCode = tx.To;

                                if (foundCode != null && foundCode.Length > 0 && subiekt.TowaryManager.IstniejeWg(foundCode, InsERT.TowarParamWyszukEnum.gtaTowarWszystko))
                                {
                                    tw = subiekt.TowaryManager.WczytajTowarWg(foundCode, InsERT.TowarParamWyszukEnum.gtaTowarWszystko);
                                    result.Warnings.Add($"Product(Code={foundCode}) has been translated (Source={o.Source}, From={p.Code})");
                                }

                                p.Code = foundCode;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"{p} does not exist in SGT");
                    }

                    if (tw.Aktywny == false)
                    {
                        throw new ArgumentException($"Produkt '{tw.Symbol}' jest nieaktywny. Nie można dodać zamówienia");
                    }

                    if (index >= order.Pozycje.Liczba)
                    {
                        result.Changes.Add($"Item(Code={tw.Symbol}, Quantity={p.Quantity}) added.");
                        SuPozycja pozNew = order.Pozycje.Dodaj(tw.Identyfikator);
                        pozNew.IloscJm = p.Quantity;

                        if ((InsERT.DokVatTransakcjaVATEnum)order.TransakcjaRodzajOperacjiVat == InsERT.DokVatTransakcjaVATEnum.gtaDokVatTransSprzedazKrajowa)
                        {
                            pozNew.VatId = TAX_TYPE.VAT_23;
                        }
                        else
                        {
                            pozNew.VatId = TAX_TYPE.VAT_0;
                        }
                    }

                    SuPozycja poz = order.Pozycje.Wczytaj(index + 1);

                    if (poz.IloscJm != (decimal)p.Quantity)
                    {
                        result.Changes.Add($"Item(Code={poz.TowarSymbol}).Quantity: {poz.IloscJm} => {p.Quantity}");
                        poz.IloscJm = (decimal)p.Quantity;
                    }

                    if ((DokVatTransakcjaVATEnum)order.TransakcjaRodzajOperacjiVat == DokVatTransakcjaVATEnum.gtaDokVatTransSprzedazKrajowa)
                    {
                        if ((TAX_TYPE)poz.VatId != TAX_TYPE.VAT_23)
                        {
                            result.Changes.Add($"Item(Code={poz.TowarSymbol}).Tax: {poz.VatId} => {TAX_TYPE.VAT_23}");
                            poz.VatId = TAX_TYPE.VAT_23;
                        }
                    }
                    else
                    {
                        if ((TAX_TYPE)poz.VatId != TAX_TYPE.VAT_0)
                        {
                            result.Changes.Add($"Item(Code={poz.TowarSymbol}).Tax: {poz.VatId} => {TAX_TYPE.VAT_0}");
                            poz.VatId = TAX_TYPE.VAT_UE;
                        }
                    }

                    if ((poz.Opis is DBNull && p.Descrption != "") || (!(poz.Opis is DBNull) && poz.Opis != p.Descrption))
                    {
                        result.Changes.Add($"Item(Code={poz.TowarSymbol}).Description: {poz.Opis} => {p.Descrption}");
                        poz.Opis = p.Descrption;
                    }

                    tw.Zamknij();
                    index++;
                }
            }

            if (o.Products != null && o.Products.Count > 0)
            {
                // W tym momencie nie ma już pozycji, których nie powinno być w zamówieniu
                // Należy jednak sprawdzić ilości, ceny i dodać brakujące pozycje
                List<OrderItem> items = new List<OrderItem>();

                int index = 0;
                foreach (InsERT.SuPozycja poz in order.Pozycje)
                {
                    if ((InsERT.TowarRodzajEnum)poz.TowarRodzaj == InsERT.TowarRodzajEnum.gtaTowarRodzajUsluga)
                    {
                        continue;
                    }

                    InsERT.Towar tw = subiekt.TowaryManager.Wczytaj(poz.TowarId);

                    var op = o.Products[index];

                    foreach (InsERT.TwCena c in tw.Ceny)
                    {
                        if (c.Id == order.PoziomCenyId)
                        {
                            var item = new OrderItem()
                            {
                                Id = poz.Id,
                                Code = tw.Symbol,
                                Ean = tw.KodyKreskowe.Podstawowy,
                                CurrentCnt = poz.Ilosc,
                                CurrentPrice = poz.CenaBruttoPoRabacie,
                                Cnt = op.Quantity,
                                PriceProduct = c.Brutto,
                                PriceCurrent = new OrderItemPrice() { Name = "Cena bieżąca ZK", Brutto = c.Brutto, Disocunt = 0m }
                            };

                            if (op.Price != null && op.Price > 0)
                            {
                                if (op.Price_drop.HasValue && op.Price_drop > 0)
                                {
                                    item.PriceOrder = new OrderItemPrice() { Name = "Cena z zamówienia", Brutto = (decimal)op.Price, Disocunt = op.Price_drop.Value };
                                }
                                else
                                {
                                    item.PriceOrder = new OrderItemPrice() { Name = "Cena z zamówienia", Brutto = (decimal)op.Price, Disocunt = 0m };
                                }
                            }

                            items.Add(item);
                            break;
                        }
                    }

                    tw.Zamknij();
                    index++;
                }

                // Przeliczanie rabatu wg Kontrahenta
                RecalculateDiscountClient(ref order, items);

                // Przeliczanie rabatu wg prduktu
                RecalculateDiscountProductDefault(ref order, items);

                // Przeliczanie rabatu wg promocji
                RecalculateDiscountPriceDrop(ref order, items);

                // Ustawianie rabatu na 0%
                order.PrzeliczRabatWg(InsERT.TypRabatuEnum.gtaTypRabatuProcent, 0);

                foreach (InsERT.SuPozycja poz in order.Pozycje)
                {
                    if ((InsERT.TowarRodzajEnum)poz.TowarRodzaj == InsERT.TowarRodzajEnum.gtaTowarRodzajUsluga)
                        continue;

                    var item = items.Find(x => x.Id == poz.Id);

                    if ((100m - item.PriceFinal.Disocunt) / 100m * item.PriceFinal.Brutto != item.CurrentPrice)
                    {
                        result.Changes.Add($"Item({item.Code}).Price: {item.CurrentPrice} => {item.PriceFinal} [{item.PriceFinal.Name}]");
                    }

                    poz.CenaBruttoPrzedRabatem = item.PriceFinal.Brutto;
                    poz.RabatProcent = item.PriceFinal.Disocunt;
                }
            }

            if (o.Delivery != null && 1 == 2)
            {
                if (o.Delivery.Cost_mode == DELIVERY_COST_MODE.ADD && o.Delivery.Price > 0)
                {
                    if (transportServiceItemId > 0)
                    {
                        if (o.Delivery.Price != currentTransportPrice)
                        {
                            // Transport był wcześniej,
                            // ma być dodany do zamówienia
                            // cena transportu się różni
                            // ===> Popraw cenę

                            InsERT.SuPozycja transport = order.Pozycje.Wczytaj(transportServiceItemId);

                            result.Changes.Add($"Delivery cost: {currentTransportPrice} => {o.Delivery.Price}");

                            transport.IloscJm = 1;
                            transport.CenaBruttoPoRabacie = currentTransportPrice;
                            transport.RabatProcent = 0;
                            transport.CenaBruttoPrzedRabatem = o.Delivery.Price;
                        }
                    }
                    else
                    {
                        if (o.Delivery.Price != currentTransportPrice)
                        {
                            // Transport był wcześniej,
                            // ma być dodany do zamówienia
                            // ===> Sprawdź cenę

                            InsERT.SuPozycja transport = order.Pozycje.Wczytaj(transportServiceItemId);
                            transport.IloscJm = 1;
                            transport.CenaBruttoPoRabacie = currentTransportPrice;
                            if (currentTransportPrice != o.Delivery.Price)
                            {
                                result.Changes.Add($"Delivery cost: {currentTransportPrice} => {o.Delivery.Price}");
                                transport.RabatProcent = 0;
                                transport.CenaBruttoPrzedRabatem = o.Delivery.Price;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Aktulizuje numer telefonu klienta
        /// </summary>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateBuyerPhone(Subiekt subiekt, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (o.Buyer != null && o.Buyer.Phone != null)
            {
                if (client.Telefony.Liczba < 1)
                {
                    result.Changes.Add($"Client.Phone: {o.Buyer.Phone}");
                    client.Telefony.Dodaj(o.Buyer.Phone);

                    int id = client.Identyfikator;

                    client.Zapisz();
                    client.Zamknij();

                    client = subiekt.KontrahenciManager.Wczytaj(id);
                }
                else
                {
                    InsERT.KhTelefon tel = client.Telefony.Wczytaj(1);
                    if (tel.Numer != o.Buyer.Phone)
                    {
                        result.Changes.Add($"Client.Phone: {tel.Numer} => {o.Buyer.Phone}");
                        tel.Numer = o.Buyer.Phone;
                        tel.Nazwa = "Telefon kontaktowy";
                    }

                    int id = client.Identyfikator;

                    client.Zapisz();
                    client.Zamknij();

                    client = subiekt.KontrahenciManager.Wczytaj(id);
                }
            }
        }

        /// <summary>
        /// Aktualizuje login klienta
        /// </summary>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateBuyerLogin(Subiekt subiekt, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (o.Buyer != null && o.Buyer.Login != null)
            {
                if (client.Skype != o.Buyer.Login)
                {
                    result.Changes.Add($"Client.Skype: {client.Skype} => {o.Buyer.Login}");
                    client.Skype = o.Buyer.Login;

                    int id = client.Identyfikator;

                    client.Zapisz();
                    client.Zamknij();

                    client = subiekt.KontrahenciManager.Wczytaj(id);
                }
            }
        }

        /// <summary>
        /// Aktualizuje email klienta
        /// </summary>
        /// <param name="client"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateBuyerEmail(Subiekt subiekt, ref Kontrahent client, Order o, UpdateResult result)
        {
            if (o.Buyer != null && o.Buyer.Email != null)
            {
                if (client.Email is DBNull || client.Email != o.Buyer.Email)
                {
                    result.Changes.Add($"Client.Email: {client.Email} => {o.Buyer.Email}");
                    client.Email = o.Buyer.Email;
                    int id = client.Identyfikator;

                    client.Zapisz();
                    client.Zamknij();

                    client = subiekt.KontrahenciManager.Wczytaj(id);
                }
            }
        }

        /// <summary>
        /// Aktualizuje walutę na zamówieniu
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void SetOrderCurrency(ref SuDokument order, Order o, UpdateResult result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Aktualizuje adres w danych do faktury dla klienta danego zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderInvoiceAddress(Subiekt subiekt, ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Invoice != null && o.Invoice.Address != null)
            {
                UpdateClientinvoiceAddress(subiekt, order.KontrahentId, o.Invoice.Address);
            }
        }

        /// <summary>
        /// Aktualizuje adres dostawy zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderDeliveryAddress(Subiekt subiekt, ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Delivery != null && o.Delivery.Code != null)
            {
                int deliveryAddressId = GetClientDeliveryAddress(subiekt, order.KontrahentId, o.Delivery.Code);
                order.AdresDostawyId = deliveryAddressId;
            }
        }

        private static void UpdateClientinvoiceAddress(Subiekt subiekt, int clientId, Address address)
        {
            InsERT.Kontrahent client = subiekt.Kontrahenci.Wczytaj(clientId);

            bool changes = false;

            if (client.Ulica != address.Street)
            {
                Console.WriteLine($"Client(Id={client.Identyfikator}).Invoice.Street: {client.Ulica} => {address.Street}");
                client.Ulica = address.Street;
                changes = true;
            }

            if (client.Miejscowosc != address.City)
            {
                Console.WriteLine($"Client(Id={client.Identyfikator}).Invoice.City: {client.Miejscowosc} => {address.City}");
                client.Miejscowosc = address.City;
                changes = true;
            }

            if (client.KodPocztowy != address.Postcode)
            {
                Console.WriteLine($"Client(Id={client.Identyfikator}).Invoice.Postcode: {client.KodPocztowy} => {address.Postcode}");
                client.KodPocztowy = address.Postcode;
                changes = true;
            }

            if (changes)
            {
                client.Zapisz();
            }

            client.Zamknij();
        }

        private static int GetClientDeliveryAddress(Subiekt subiekt, int clientId, string code)
        {
            InsERT.Kontrahent client = subiekt.KontrahenciManager.Wczytaj(clientId);
            int id = 0;

            foreach (InsERT.KhAdresDostawy address in client.AdresyDostaw)
            {
                if (address.Nazwa == code)
                {
                    return address.Id;
                }
            }

            string clientCode = client.NazwaPelna;
            client.Zamknij();

            throw new NullReferenceException($"Address(Code={code}) does not exist in Client(Id={clientId}, Code={clientCode})");
        }

        /// <summary>
        /// Aktualizuje metodę dostawy zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderDeliveryMethod(SubiektGT worker, ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Delivery != null && o.Delivery.Method != null)
            {
                if (order.PoleWlasne[worker.SGT_ORDER_CUSTOMFIELD_COURIER_NAME] is DBNull || o.Delivery.Method != order.PoleWlasne[worker.SGT_ORDER_CUSTOMFIELD_COURIER_NAME])
                {
                    result.Changes.Add($"Delivery: {order.PoleWlasne[worker.SGT_ORDER_CUSTOMFIELD_COURIER_NAME]} => {o.Delivery.Method}");
                    order.PoleWlasne[worker.SGT_ORDER_CUSTOMFIELD_COURIER_NAME] = o.Delivery.Method;
                }
            }
        }

        public class UpdateResult
        {
            public List<string> Changes = new List<string>();
            public List<string> Warnings = new List<string>();
            public List<object> Errors = new List<object>();
            public OrderConfirmation Confimation { get; set; }
        }

        /// <summary>
        /// Aktualizuje uwagi zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderBuyerComment(ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Buyer != null && o.Buyer.Comment != null)
            {
                if (order.Uwagi is DBNull || o.Buyer.Comment != FixString(order.Uwagi))
                {
                    string fixedComment = FixString(o.Buyer.Comment);
                    result.Changes.Add($"Comment: {order.Uwagi} => {fixedComment}");
                    order.Uwagi = fixedComment;
                }
            }
        }

        /// <summary>
        /// Aktualizuje termin realizacji zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderDeadline(ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Deadline != null)
            {
                if (order.TerminRealizacji is DBNull || o.Deadline != order.TerminRealizacji)
                {
                    result.Changes.Add($"Deadline: {order.TerminRealizacji} => {o.Deadline}");
                    order.TerminRealizacji = o.Deadline;
                }
            }
        }

        /// <summary>
        /// Aktualizuje datę dodania zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderCreateTime(ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Created_at != null)
            {
                if (order.DataWystawienia is DBNull || o.Created_at.Value.Date != order.DataWystawienia)
                {
                    result.Changes.Add($"Date of create: {order.DataWystawienia} => {o.Created_at}");
                    order.DataWystawienia = o.Created_at;
                    order.DataMagazynowa = o.Created_at;
                }
            }
        }

        /// <summary>
        /// Aktualizuje podtytuł zamówienia
        /// </summary>
        /// <param name="order"></param>
        /// <param name="o"></param>
        /// <param name="result"></param>
        private static void UpdateOrderSubtitle(ref SuDokument order, Order o, UpdateResult result)
        {
            if (o.Subtitle != null)
            {
                if (order.Podtytul is DBNull || o.Subtitle != order.Podtytul)
                {
                    result.Changes.Add($"Subtitle: {order.Podtytul} => {o.Subtitle}");
                    order.Podtytul = o.Subtitle;
                }
            }


        }

        /// <summary>
        /// Przelicza rabat wg produktu
        /// </summary>
        /// <param name="order"></param>
        /// <param name="items"></param>
        private static void RecalculateDiscountProductDefault(ref InsERT.SuDokument order, List<OrderItem> items)
        {
            order.PrzeliczRabatWg(InsERT.TypRabatuEnum.gtaTypRabatuTowar);
            foreach (InsERT.SuPozycja poz in order.Pozycje)
            {
                OrderItem item = items.Find(x => x.Id == poz.Id);
                if (item != default(OrderItem))
                {
                    item.PriceProductDefaultDrop = poz.CenaBruttoPoRabacie;
                    item.PriceProductDrop = new OrderItemPrice() { Name = "Rabat dla produktu", Brutto = poz.CenaBruttoPoRabacie, Disocunt = (decimal)poz.RabatProcent };
                }
            }
        }

        /// <summary>
        /// Przelicza rabat dla promocji
        /// </summary>
        /// <param name="order"></param>
        /// <param name="items"></param>
        private static void RecalculateDiscountPriceDrop(ref InsERT.SuDokument order, List<OrderItem> items)
        {
            order.PrzeliczRabatWg(InsERT.TypRabatuEnum.gtaTypRabatuPromocja);
            foreach (InsERT.SuPozycja poz in order.Pozycje)
            {
                OrderItem item = items.Find(x => x.Id == poz.Id);
                if (item != default(OrderItem))
                {
                    item.PricePromotion = poz.CenaBruttoPoRabacie;
                    item.PriceAvailablePromotion = new OrderItemPrice() { Brutto = poz.CenaBruttoPoRabacie, Disocunt = (decimal)poz.RabatProcent };
                }
            }
        }

        /// <summary>
        /// Przelicza rabat wg kontrahenta
        /// </summary>
        /// <param name="order"></param>
        /// <param name="items"></param>
        private static void RecalculateDiscountClient(ref InsERT.SuDokument order, List<OrderItem> items)
        {
            order.PrzeliczRabatWg(InsERT.TypRabatuEnum.gtaTypRabatuKontrahent);
            foreach (InsERT.SuPozycja poz in order.Pozycje)
            {
                OrderItem item = items.Find(x => x.Id == poz.Id);
                if (item != default(OrderItem))
                {
                    item.PriceClientDefaultDrop = poz.CenaBruttoPoRabacie;
                    item.PriceClientDrop = new OrderItemPrice() { Name = "Rabat dla klienta", Brutto = poz.CenaBruttoPoRabacie, Disocunt = (decimal)poz.RabatProcent };
                }
            }
        }

        /// <summary>
        /// Check if document with (source, ext_number) exist
        /// </summary>
        /// <returns></returns>
        public static bool Exist(SubiektGT worker, string source, string externalNumber)
        {
            var predicate = BuildFilter("pwd_Tekst02", source, externalNumber);
            return worker.db.vwPolaWlasne_Dokument.FirstOrDefault(predicate) != default;
        }

        /// <summary>
        /// Zwraca Id dokumentu (dok_Id) na podstawie pary (Source, ExternalOrderNumber)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="externalNumber"></param>
        /// <returns></returns>
        public static int GetId(SubiektGT worker, string source, string externalNumber)
        {
            var predicate = BuildFilter("pwd_Tekst02", source, externalNumber);
            return worker.db.vwPolaWlasne_Dokument.FirstOrDefault(predicate).dok_Id;
        }

        /// <summary>
        /// Sets order flag
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> SetFlagById(int id, string flag, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                if(!subiekt.SuDokumentyManager.Istnieje(id))
                {
                    return TypedResults.BadRequest("Document not found");
                }

                SuDokument doc = subiekt.SuDokumentyManager.Wczytaj(id);

                if(doc.Typ != (int)SuDokumentTypEnum.gtaSuDokumentTypZK)
                {
                    return TypedResults.BadRequest("Invalid document type");
                }

                try
                {
                    doc.FlagaNazwa = flag;
                    doc.FlagaKomentarz = "testowy komentarz, flaga " + flag;
                    doc.Zapisz();
                }
                catch (COMException ex)
                {
                    Console.WriteLine(ex.Message);
                    return TypedResults.BadRequest(ex.Message);
                }
                

                return TypedResults.Ok(id);
            });
        }

        /// <summary>
        /// Delete order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> Delete(int id, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                return TypedResults.UnprocessableEntity();
            });
        }

        /// <summary>
        /// Delete order
        /// </summary>
        /// <param name="source"></param>
        /// <param name="externalNumber"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> DeleteExternal(string source, string externalNumber, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                return TypedResults.UnprocessableEntity();
            });
        }

        /// <summary>
        /// Realize order by WZ document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public static async Task<IResult> Realize(int id, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                return TypedResults.UnprocessableEntity();
            });
        }

        /// <summary>
        /// Realize order by WZ document
        /// </summary>
        /// <param name="source"></param>
        /// <param name="externalNumber"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public static async Task<IResult> RealizeExternal(string source, string externalNumber, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                var predicate = BuildFilter("pwd_Tekst02", source, externalNumber);
                var res = worker.db.vwPolaWlasne_Dokument.FirstOrDefault(predicate);

                if(res == default)
                {
                    return TypedResults.BadRequest("Order not found");
                }

                SuDokument order = subiekt.SuDokumentyManager.Wczytaj(res.dok_Id);

                SuDokument wz = subiekt.SuDokumentyManager.DodajWZ();
                wz.NaPodstawie(order);

                try
                {
                    wz.ZapiszSymulacja();
                }
                catch(COMException ex)
                {
                    if(ex.Message.Contains("0x800411DC"))
                    {
                        return TypedResults.BadRequest("Order already realized");
                    }
                }
                    
                wz.Zapisz();

                int wzId = wz.Identyfikator;
                string wzNumber = wz.NumerPelny;

                wz.Zamknij();
                order.Zamknij();

                return TypedResults.Ok($"Dodano {wzNumber} (Id={wzId})");
            });
        }

        /// <summary>
        /// Get SKU translation from given source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<IResult> GetTranslation(string source, string from, TxContext db)
        {
            var res = db.TxItems.FirstOrDefault(x => x.Source == source && x.From == from);
            if(res == default)
                return TypedResults.NotFound();

            return TypedResults.Ok(res);
        }

        public static async Task<IResult> GetTranslations(TxContext db)
        {
            return TypedResults.Ok(db.TxItems.ToList());
        }

        /// <summary>
        /// Add or update SKU translation from given source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static async Task<IResult> UpdateTranslation(string source, string from, string to, TxContext db)
        {
            var res = db.TxItems.FirstOrDefault(x => x.Source == source && x.From == from);
            if (res == default)
            {
                db.TxItems.Add(new Tx { From = from, To = to, Source = source });
                await db.SaveChangesAsync();
                return TypedResults.Ok("Added");
            }

            res.To = to;

            await db.SaveChangesAsync();

            return TypedResults.Ok("Updated");
        }

        /// <summary>
        /// Delete SKU translation from given source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<IResult> DeleteTranslation(string source, string from, TxContext db)
        {
            var res = db.TxItems.FirstOrDefault(x => x.Source == source && x.From == from);
            if (res == default)
            {
                return TypedResults.BadRequest("Translation not found");
            }

            db.Remove(res);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }

        /// <summary>
        /// Delete SKU translation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>

        public static async Task<IResult> DeleteTranslationById(int id, TxContext db)
        {
            var res = db.TxItems.FirstOrDefault(x=>x.Id == id);
            if (res == default)
            {
                return TypedResults.BadRequest("Translation not found");
            }

            db.Remove(res);
            await db.SaveChangesAsync();

            return TypedResults.Ok();

        }
    }
}
