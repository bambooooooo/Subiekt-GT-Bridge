using InsERT;
using SGT_BRIDGE.Models.Product;
using SGT_BRIDGE.Services;

namespace SGT_BRIDGE.Endpoints
{
    public static class PackageEndpoint
    {
        public static void RegisterPackageEndpoint(this WebApplication app)
        {
            var items = app.MapGroup("/packages");

            items.MapGet("/{code}", Get);
            items.MapPost("/", Post);
            items.MapDelete("/{code}", Delete);
        }

        /// <summary>
        /// Retrives package data
        /// </summary>
        /// <param name="code"></param>
        /// <param name="worker"></param>
        /// <response code="200">Package data</response>
        /// <response code="400">If not found</response>
        public async static Task<IResult> Get(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                if(!subiekt.TowaryManager.IstniejeWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    return TypedResults.NotFound();
                }

                Towar package = subiekt.TowaryManager.WczytajTowarWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu);

                Package p = new()
                {
                    Id = package.Symbol,
                    Key = package.Nazwa,
                    Barcode = package.KodyKreskowe.Podstawowy,
                    Length = (package.PoleWlasne[worker.LENGTH_FIELD_NAME] is System.DBNull) ? 0 : (decimal)package.PoleWlasne[worker.LENGTH_FIELD_NAME],
                    Width = (package.PoleWlasne[worker.WIDTH_FIELD_NAME] is System.DBNull) ? 0 : (decimal)package.PoleWlasne[worker.WIDTH_FIELD_NAME],
                    Height = (package.PoleWlasne[worker.HEIGHT_FIELD_NAME] is System.DBNull) ? 0 : (decimal)package.PoleWlasne[worker.HEIGHT_FIELD_NAME],
                    Mass = (package.Masa is System.DBNull) ? 0 : (decimal)package.Masa
                };

                package.Zamknij();

                return TypedResults.Ok(p);
            });
        }

        /// <summary>
        /// Add or edit package data
        /// </summary>
        /// <param name="p"></param>
        /// <param name="worker"></param>
        /// <response code="200">Internal Id of new object</response>
        /// <response code="422">If package is locked or uneditable</response>
        /// <response code="400">If package data is invalid. For example: Main barcode is already taken</response>
        public async static Task<IResult> Post(Package p, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                Towar tw;

                if (sgt.TowaryManager.IstniejeWg(p.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    tw = sgt.TowaryManager.WczytajTowarWg(p.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu);
                    if(!tw.MoznaEdytowac)
                    {
                        return TypedResults.UnprocessableEntity($"Package is not editable");
                    }
                }
                else
                {
                    tw = sgt.TowaryManager.DodajTowar();
                    tw.Symbol = p.Id;
                    tw.OznaczenieJpkVat = OznaczenieTowJpkVatEnum.gtaOznaczTowJpkVat00_NieOznaczaj;
                }

                if (p.Key != null)
                    tw.Nazwa = p.Key;

                if (p.Volume > 0)
                {
                    if (tw.Objetosc is DBNull || tw.Objetosc == null || Math.Round(tw.Objetosc, 3) != Math.Round(p.Volume, 3))
                    {
                        tw.Objetosc = Math.Round((decimal)p.Volume, 3);
                    }
                }

                if (p.Length > 0)
                {
                    tw.PoleWlasne[worker.LENGTH_FIELD_NAME] = p.Length;
                }

                if (p.Width > 0)
                {
                    tw.PoleWlasne[worker.WIDTH_FIELD_NAME] = p.Width;
                }

                if (p.Height > 0)
                {
                    tw.PoleWlasne[worker.HEIGHT_FIELD_NAME] = p.Height;
                }

                if (p.Mass > 0)
                {
                    tw.Masa = p.Mass;
                }

                if (p.Barcode != null && p.Barcode.Length > 1)
                {
                    if (tw.KodyKreskowe.Podstawowy is DBNull || tw.KodyKreskowe.Podstawowy != p.Barcode)
                    {
                        try
                        {
                            tw.KodyKreskowe.Podstawowy = p.Barcode;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            return TypedResults.BadRequest($"Cannot add main barcode");
                        }
                    }
                }

                if(p.BasePrice > 0)
                {
                    tw.CenaKartotekowa = p.BasePrice;
                }

                tw.Zapisz();

                int id = tw.Identyfikator;
                tw.Zamknij();

                return TypedResults.Ok(id);
            });
        }

        /// <summary>
        /// Delete package
        /// </summary>
        /// <param name="code"></param>
        /// <param name="worker"></param>
        /// <response code="200">If package deleted</response>
        /// <response code="400">If package does not exist</response>
        /// <response code="409">If package can not be deleted</response>
        /// <returns></returns>
        public async static Task<IResult> Delete(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                Towar tw;

                if (!sgt.TowaryManager.IstniejeWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    return TypedResults.NotFound();
                }

                tw = sgt.TowaryManager.WczytajTowarWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu);

                if (tw.MoznaUsunac)
                {
                    tw.Usun();
                    return TypedResults.Ok();
                }

                return TypedResults.Conflict("Can not delete object");
            });
        }
    }
}
