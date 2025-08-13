using Microsoft.EntityFrameworkCore;
using SGT_BRIDGE.Models;
using SGT_BRIDGE.Services;

namespace SGT_BRIDGE.Endpoints
{
    public static class PriceEndpoint
    {
        public static void RegisterPriceEndpoint(this WebApplication app)
        {
            var items = app.MapGroup("/prices");
            items.MapGet("/", GetAll);
            items.MapGet("/{code}", Get);
            items.MapPost("/", Post);
            items.MapDelete("/{code}", Delete);
        }

        public async static Task<IResult> Get(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                var data = worker.db.LEO_SystemRabatowy_Zestawy.Where(x => x.zr_Symbol == code).First();

                return TypedResults.Ok(new Price()
                {
                    Id = data.zr_Id,
                    Code = data.zr_Symbol,
                    Name = data.zr_Nazwa
                });
            });
        }

        public async static Task<IResult> GetAll(SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                var data = worker.db.LEO_SystemRabatowy_Zestawy.Select(x => new Price()
                {
                    Id = x.zr_Id,
                    Code = x.zr_Symbol,
                    Name = x.zr_Nazwa
                });

                return TypedResults.Ok(data);
            });
        }

        public async static Task<IResult> Post(Price p, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                var obj = worker.db.LEO_SystemRabatowy_Zestawy.Where(x=>x.zr_Symbol == p.Code).FirstOrDefault();

                if(obj == default)
                {
                    var warehouses = worker.db.sl_Magazyn.Select(x=>x.mag_Id).ToList();

                    var newPrice = new LEO_SystemRabatowy_Zestawy()
                    {
                        zr_Symbol = p.Code,
                        zr_Nazwa = p.Name,
                        zr_Aktywny = true,
                        zr_PoziomCeny = 0,
                        zr_OstatniaModyfikacja = DateTime.Now,
                        zr_NotatkiEnabled = false,
                        zr_Notatki = "",
                        zr_DodatkowyRabatStalejCenyZestaw = true,
                        zr_DodatkowyRabatUpustKwotowyZestaw = true,
                        zr_DodatkowyRabatUpustProcentowyZestaw =  true,
                    };

                    worker.db.LEO_SystemRabatowy_Zestawy.Add(newPrice);
                    worker.db.SaveChanges();

                    foreach(var warehouseId in warehouses)
                    {
                        worker.db.LEO_SystemRabatowy_ZestawyMagazyny.Add(new LEO_SystemRabatowy_ZestawyMagazyny()
                        {
                            zrm_MagazynId = warehouseId,
                            zrm_ZestawId = newPrice.zr_Id
                        });
                    }

                    worker.db.SaveChanges();

                    return TypedResults.Ok();
                }
                else
                {
                    obj.zr_Nazwa = p.Name;
                    worker.db.SaveChanges();

                    return TypedResults.Ok();
                }
            });
        }

        public async static Task<IResult> Delete(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                var z = worker.db.LEO_SystemRabatowy_Zestawy.FirstOrDefault(x => x.zr_Symbol == code);
                if (z == default)
                {
                    return TypedResults.BadRequest("Price not found");
                }

                worker.db.LEO_SystemRabatowy_ZestawyKontrahenci.Where(x => x.zrk_ZestawId == z.zr_Id).ExecuteDelete();
                worker.db.LEO_SystemRabatowy_ZestawyGrupyKh.Where(x=>x.zrg_ZestawId==z.zr_Id).ExecuteDelete();
                worker.db.LEO_SystemRabatowy_ZestawyMagazyny.Where(x => x.zrm_ZestawId == z.zr_Id).ExecuteDelete();
                worker.db.LEO_SystemRabatowy_ZestawyPowiazania.Where(x => x.zrp_ZestawId == z.zr_Id).ExecuteDelete();
                worker.db.LEO_SystemRabatowy_Zestawy.Where(x=>x.zr_Id == z.zr_Id).ExecuteDelete();

                return TypedResults.Ok();
            });
        }
    }
}
