using InsERT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGT_BRIDGE.Models;
using SGT_BRIDGE.Services;
using System.Runtime.InteropServices;

namespace SGT_BRIDGE.Endpoints
{
    public static class UserEndpoint
    {
        public static void RegisterUserEndpoint(this WebApplication app)
        {
            var items = app.MapGroup("/users");
            items.MapGet("/{vat}", Get);
            items.MapPost("/", Post);
            items.MapDelete("/{vat}", Delete);
        }

        public async static Task<IResult> Get(string vat, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                if(!sgt.KontrahenciManager.IstniejeWg(vat, KontrahentParamWyszukEnum.gtaKontrahentWgNip))
                {
                    return TypedResults.BadRequest("User not found.");
                }

                Kontrahent kh = sgt.KontrahenciManager.WczytajKontrahentaWg(vat, KontrahentParamWyszukEnum.gtaKontrahentWgNip);

                int khId = kh.Identyfikator;
                var priceLevel = worker.db.LEO_SystemRabatowy_ZestawyKontrahenci.FirstOrDefault(x => x.zrk_KontrahentId == khId);

                string priceLevelId = String.Empty;

                if (priceLevel != default)
                {
                    var pl = worker.db.LEO_SystemRabatowy_Zestawy.First(x => x.zr_Id == priceLevel.zrk_ZestawId);
                    priceLevelId = pl.zr_Symbol;
                }

                var user = new User
                {
                    Id = kh.Identyfikator.ToString(),
                    Name = kh.Symbol,
                    VAT = kh.NIP,
                    PriceLevel = priceLevelId
                };

                return TypedResults.Ok(user);

            });
        }

        public async static Task<IResult> Post(User p, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                Kontrahent kh;
                int clientId;

                if (!subiekt.KontrahenciManager.IstniejeWg(p.VAT, KontrahentParamWyszukEnum.gtaKontrahentWgNip))
                {
                    try
                    {
                        kh = subiekt.KontrahenciManager.DodajKontrahenta();
                        kh.NIP = p.VAT;
                        kh.Symbol = p.Name;
                        kh.Nazwa = p.Name;
                        kh.Zapisz();
                    }
                    catch(COMException ex)
                    {
                        if(ex.Message.Contains("0x800414C0"))
                        {
                            return TypedResults.BadRequest("User VAT number already taken");
                        }

                        throw ex;
                    }
                }
                else
                {
                    kh = subiekt.KontrahenciManager.WczytajKontrahentaWg(p.VAT, KontrahentParamWyszukEnum.gtaKontrahentWgNip);
                }

                clientId = (int)kh.Identyfikator;
                kh.Zamknij();

                var newPriceLevel = worker.db.LEO_SystemRabatowy_Zestawy
                    .FirstOrDefault(x => x.zr_Symbol == p.PriceLevel);

                if(newPriceLevel != default)
                {
                    var userPrices = worker.db.LEO_SystemRabatowy_ZestawyKontrahenci
                        .Where(x => x.zrk_KontrahentId == clientId).ToList();

                    bool change = false;
                    bool foundValid = false;

                    foreach (var userPrice in userPrices)
                    {
                        if (userPrice.zrk_ZestawId == newPriceLevel.zr_Id)
                        {
                            foundValid = true;
                        }
                        else
                        {
                            worker.db.LEO_SystemRabatowy_ZestawyKontrahenci.Where(x => x.zrk_ZestawId == userPrice.zrk_ZestawId && x.zrk_KontrahentId == clientId).ExecuteDelete();
                            change = true;
                        }
                    }

                    if(change)
                        worker.db.SaveChanges();

                    if (!foundValid)
                    {
                        worker.db.ChangeTracker.Clear();

                        var newRecord = worker.db.LEO_SystemRabatowy_ZestawyKontrahenci.Add(new LEO_SystemRabatowy_ZestawyKontrahenci()
                        {
                            zrk_ZestawId = newPriceLevel.zr_Id,
                            zrk_KontrahentId = clientId,
                        });

                        change = true;
                    }

                    if(change)
                        worker.db.SaveChanges();
                }

                return TypedResults.Ok();
            });
        }

        public async static Task<IResult> Delete(string vat, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                if (!sgt.KontrahenciManager.IstniejeWg(vat, KontrahentParamWyszukEnum.gtaKontrahentWgNip))
                {
                    return TypedResults.BadRequest("User not found.");
                }

                Kontrahent kh = sgt.KontrahenciManager.WczytajKontrahentaWg(vat, KontrahentParamWyszukEnum.gtaKontrahentWgNip);

                if(kh.MoznaUsunac)
                {
                    kh.Usun();
                    return TypedResults.Ok();
                }

                return TypedResults.BadRequest("Can not delete");
                    
            });
        }
    }
}
