using InsERT;
using Microsoft.EntityFrameworkCore;
using SGT_BRIDGE.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    }
}
