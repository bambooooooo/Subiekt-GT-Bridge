using InsERT;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGT_BRIDGE.Models;
using SGT_BRIDGE.Models.Order;
using System.Threading.Channels;

namespace SGT_BRIDGE.Services
{
    public class SubiektGT : IAsyncDisposable
    {
        public enum SGT_DOC_TYPE
        {
            FZ = 1,
            FS = 2,
            KFZ = 5,
            KFS = 6,
            MM = 9,
            PZ = 10,
            WZ = 11,
            PW = 12,
            RW = 13,
            ZW = 14,
            ZD = 15,
            ZK = 16,
            PA = 21,
            IW = 29,
            ZPZ = 35,
            ZWZ = 36,
            FM = 62
        }

        public static readonly List<SGT_DOC_TYPE> SGT_DOC_TYPE_MAG = [
            SGT_DOC_TYPE.MM,
            SGT_DOC_TYPE.PZ,
            SGT_DOC_TYPE.WZ,
            SGT_DOC_TYPE.PW,
            SGT_DOC_TYPE.RW,
            SGT_DOC_TYPE.IW,
            SGT_DOC_TYPE.ZPZ,
            SGT_DOC_TYPE.ZWZ
        ];

        public static readonly List<SGT_DOC_TYPE> SGT_DOC_TYPE_HAN = [
            SGT_DOC_TYPE.FZ,
            SGT_DOC_TYPE.FS,
            SGT_DOC_TYPE.KFZ,
            SGT_DOC_TYPE.KFS,
            SGT_DOC_TYPE.ZD,
            SGT_DOC_TYPE.ZK,
            SGT_DOC_TYPE.PA,
            SGT_DOC_TYPE.FM
        ];

        public readonly string LENGTH_FIELD_NAME;
        public readonly string WIDTH_FIELD_NAME;
        public readonly string HEIGHT_FIELD_NAME;
        public readonly string NAME_EN_FIELD_NAME;
        public readonly string SOURCE_FIELD_NAME;
        public readonly string SGT_ORDER_CUSTOMFIELD_COURIER_NAME;
        public readonly int SGT_TRANSPORT_SERVICE_ID;

        private readonly Channel<SubiektRequest<object>> _channel = Channel.CreateUnbounded<SubiektRequest<object>>();
        private readonly Thread _workerThread;
        private InsERT.Subiekt? _subiekt;
        
        public SubiektGTDbContext? db;

        private readonly CancellationTokenSource _cts = new();

        readonly IConfiguration _config;

        public SubiektGT(IConfiguration configuration)
        {
            _config = configuration;

            LENGTH_FIELD_NAME = _config["SGT:Product:Customfield:Depth"] ?? "Głębokość";
            WIDTH_FIELD_NAME = _config["SGT:Product:Customfield:Width"] ?? "Szerokość";
            HEIGHT_FIELD_NAME = _config["SGT:Product:Customfield:Height"] ?? "Wysokość";
            NAME_EN_FIELD_NAME = _config["SGT:Product:NameEnFieldName"] ?? string.Empty;
            SOURCE_FIELD_NAME = _config["SGT:Order:SourceFieldName"] ?? "Źródło";
            SGT_ORDER_CUSTOMFIELD_COURIER_NAME = _config["SGT:Order:CourierName"] ?? "Kurier";
            SGT_TRANSPORT_SERVICE_ID = int.Parse(_config["SGT:Order:TransportServiceId"] ?? "0");

            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "Worker"
            };

            _workerThread.TrySetApartmentState(ApartmentState.STA);
            _workerThread.Start();
        }

        private void WorkerLoop()
        {
            try
            {
                var gt = new GT
                {
                    Serwer = _config["SGT:Server"] ?? "(local)",
                    Baza = _config["SGT:Database"] ?? "Database",
                    Uzytkownik = _config["SGT:User"] ?? "sa",
                    UzytkownikHaslo = _config["SGT:Password"] ?? "sa_password",
                    Operator = _config["SGT:Operator"] ?? "Szef",
                    OperatorHaslo = _config["SGT:OperatorPassword"] ?? "",
                    Autentykacja = _config["SGT:Auth"] == "Windows" ? AutentykacjaEnum.gtaAutentykacjaWindows : AutentykacjaEnum.gtaAutentykacjaMieszana,

                    Produkt = ProduktEnum.gtaProduktSubiekt
                };

                Console.WriteLine($"Connecting with SGT API [{gt.Operator}]{gt.Baza}@{gt.Serwer}...");

                _subiekt = (Subiekt)gt.Uruchom((int)UruchomDopasujEnum.gtaUruchomDopasujOperatora, (int)UruchomEnum.gtaUruchomNowy | (int)UruchomEnum.gtaUruchomWTle);

                string auth = (_config["SGT:Auth"] == "Windows") ? "Integrated Security=True" : $"User Id={_config["SGT:User"]};Password={_config["SGT:Password"]}";
                string connstr = $"Data Source={_config["SGT:Server"]};Initial Catalog={_config["SGT:Database"]};{auth};Encrypt=True;Trust Server Certificate=True";

                var optionsBuilder = new DbContextOptionsBuilder<SubiektGTDbContext>();
                optionsBuilder.UseSqlServer(connstr);

                Console.WriteLine($"Connectionstring from subiekt: {connstr}");

                db = new SubiektGTDbContext(optionsBuilder.Options);

                Console.WriteLine($"Connected. (PID={_subiekt.IdentyfikatorProcesu})");

                while(!_cts.IsCancellationRequested)
                {
                    if(_channel.Reader.TryRead(out var request))
                    {
                        try
                        {
                            var result = request.Request(_subiekt!);
                            request.CompletionSource.TrySetResult(result);
                        }
                        catch (Exception ex)
                        {
                            request.CompletionSource.TrySetException(ex);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("STA Thread Exception: " + ex.Message);
                throw ex;
            }
        }

        public Task<T> EnqueueAsync<T>(Func<InsERT.Subiekt, T> request)
        {
            var subiektRequest = new SubiektRequest<object>
            {
                Request = (s) => request(s)!,
            };

            var castedTcs = new TaskCompletionSource<T>();
            subiektRequest.CompletionSource.Task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    castedTcs.TrySetException(t.Exception!);
                }
                else
                {
                    castedTcs.TrySetResult((T)t.Result);
                }
            });

            _channel.Writer.TryWrite(subiektRequest);
            return castedTcs.Task;
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _channel.Writer.Complete();
            await _channel.Reader.Completion;
        }

        /// <summary>
        /// Zwraca kod ISO kraju na podstawie danego Id
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public string GetCountryIsoCode(SubiektGT worker, int countryId)
        {
            return worker.db.sl_Panstwo.FirstOrDefault(x => x.pa_Id == countryId).pa_KodPanstwaISO;
        }

        /// <summary>
        /// Zwraca Id kraju na podstawie dwuliterowego kodu
        /// </summary>
        /// <param name="delivery_country_code"></param>
        /// <returns></returns>
        public int GetCountryId(SubiektGT worker, string delivery_country_code)
        {
            return worker.db.sl_Panstwo.FirstOrDefault(x => x.pa_KodPanstwaISO == delivery_country_code).pa_Id;
        }

        /// <summary>
        /// Pobiera nowe Id klienta (Max(Id)+1 z tabeli kh__Kontrahent)
        /// </summary>
        /// <returns></returns>
        private int GetNextClientId(SubiektGT worker)
        {
            return worker.db.vwPolaWlasne_Kontrahent.Max(x => x.kh_Id) + 1;
        }

        /// <summary>
        /// Pobiera Id klienta z danych do FV. Jeśli klient nie istnieje w bazie tworzy go.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        internal int GetClient(Subiekt subiekt, SubiektGT worker, OrderInvoice invoice)
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

        /// <summary>
        /// Aktualizuje dane do faktury klienta
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="address"></param>
        public void UpdateClientinvoiceAddress(Subiekt subiekt, int clientId, Address address)
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

        /// <summary>
        /// Zwraca Id adresu dostawy klienta. Jeśli adres nie istnieje w bazie tworzy go.
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        internal int GetClientDeliveryAddress(SubiektGT worker, Subiekt subiekt, int clientId, Address delivery)
        {
            InsERT.Kontrahent client = subiekt.KontrahenciManager.Wczytaj(clientId);

            int id = 0;

            if (client.AdresyDostaw.Liczba <= 0)
            {
                InsERT.KhAdresDostawy adres = client.AdresyDostaw.Dodaj(1);
                adres.Nazwa = "Dostawa";
                adres.Ulica = delivery.Street;
                adres.KodPocztowy = delivery.Postcode;
                adres.Miejscowosc = delivery.City;
                adres.Panstwo = GetCountryId(worker, delivery.CountryCode);

                adres.UstawJakoDomyslny = true;

                client.Ulica = delivery.Street;
                client.KodPocztowy = delivery.Postcode;
                client.Miejscowosc = delivery.City;
                client.Panstwo = GetCountryId(worker, delivery.CountryCode);

                client.AdresyDostaw.Zapisz();
                client.Zapisz();

                client = subiekt.KontrahenciManager.Wczytaj(clientId);

                InsERT.KhAdresDostawy ad = client.AdresyDostaw.Wczytaj(1);
                id = ad.Id;
            }

            bool found = false;

            foreach (InsERT.KhAdresDostawy adres in client.AdresyDostaw)
            {
                if (adres.Ulica == delivery.Street
                    && adres.Miejscowosc == delivery.City
                    && adres.KodPocztowy == delivery.Postcode
                    && adres.Panstwo == GetCountryId(worker, delivery.CountryCode))
                {
                    found = true;
                    id = adres.Id;
                    break;
                }
            }

            if (!found)
            {
                InsERT.KhAdresDostawy adres = client.AdresyDostaw.Dodaj(1);
                adres.Nazwa = "Dostawa";
                adres.Ulica = delivery.Street;
                adres.KodPocztowy = delivery.Postcode;
                adres.Miejscowosc = delivery.City;
                adres.Panstwo = worker.GetCountryId(worker, delivery.CountryCode);

                adres.UstawJakoDomyslny = true;

                client.AdresyDostaw.Zapisz();
                InsERT.KhAdresDostawy ad = client.AdresyDostaw.Wczytaj(1);
                id = ad.Id;
            }

            client.Zamknij();
            return id;
        }

        internal int GetClientDeliveryAddress(Subiekt subiekt, int clientId, string code)
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
    }
}
