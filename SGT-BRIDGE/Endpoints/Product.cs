using InsERT;
using Microsoft.AspNetCore.Http.HttpResults;
using SGT_BRIDGE.Models;
using SGT_BRIDGE.Services;
using System.Drawing;
using System.Data;

namespace SGT_BRIDGE.Endpoints
{
    public static class ProductEndpoint
    {
        public static void RegisterProductEndpoint(this WebApplication app)
        {
            var items = app.MapGroup("/products");

            items.MapGet("/{code}", Get);
            items.MapPost("/", Post);
            items.MapDelete("/{code}", Delete);

            ClearTempFiles();
        }

        /// <summary>
        /// Retrieves product data
        /// </summary>
        /// <param name="code">Product identifier</param>
        /// <param name="worker"></param>
        /// <remarks>
        /// Additional desc
        /// </remarks>
        /// <response code="200">Product data</response>
        /// <response code="404">If product does not exist</response>
        public async static Task<IResult> Get(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(subiekt =>
            {
                if(!subiekt.TowaryManager.IstniejeWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    return TypedResults.NotFound("Product not found");
                }

                Towar tw = subiekt.TowaryManager.WczytajTowarWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu);

                Product p = new()
                {
                    Id = tw.Symbol,
                    Key = tw.Nazwa,
                    NamePl = tw.Opis,
                    Description = tw.Uwagi,
                    Ean = (tw.KodyKreskowe.Podstawowy is DBNull) ? null : (string)tw.KodyKreskowe.Podstawowy
                };


                List<PackageLineItem> packages = [];
                if(tw.Skladniki.Liczba > 0)
                {
                    foreach(TwSkladnik skl in tw.Skladniki)
                    {
                        Towar package = subiekt.TowaryManager.WczytajTowar(skl.TowarId);

                        packages.Add(new PackageLineItem()
                        {
                            Id = package.Symbol,
                            Quantity = (decimal)skl.Ilosc
                        });
                    }

                    p.Packages = packages;
                }

                tw.Zamknij();

                return TypedResults.Ok(p);
            });
        }

        /// <summary>
        /// Add or edit product data
        /// </summary>
        /// <param name="p"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> Post(Product p, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                Towar tw;

                if(sgt.TowaryManager.IstniejeWg(p.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    tw = sgt.TowaryManager.WczytajTowarWg(p.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu);
                }
                else
                {
                    if(p.Id == null)
                        return TypedResults.BadRequest("Can not add product without Id");

                    if (p.Key == null)
                        return TypedResults.BadRequest("Can not add product without Key");

                    tw = sgt.TowaryManager.DodajKomplet();
                    tw.Symbol = p.Id;
                    tw.Nazwa = p.Key;
                    tw.OznaczenieJpkVat = OznaczenieTowJpkVatEnum.gtaOznaczTowJpkVat00_NieOznaczaj;
                }

                if(p.Key != null)
                {
                    tw.Nazwa = p.Key;
                }
                
                if(p.NamePl != null)
                    tw.Opis = p.NamePl;

                if(p.NameEn != null)
                    tw.Uwagi = p.NameEn;

                if (p.Ean != null && p.Ean.Length > 1)
                {
                    if (tw.KodyKreskowe.Podstawowy is DBNull || tw.KodyKreskowe.Podstawowy != p.Ean)
                    {
                        try
                        {
                            tw.KodyKreskowe.Podstawowy = p.Ean;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            return TypedResults.UnprocessableEntity("Can not add main barcode");
                        }
                    }
                }

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

                if(p.Packages != null)
                {
                    List<string> validItems = p.Packages.Select(x => x.Id).ToList();

                    foreach (TwSkladnik skl in tw.Skladniki)
                    {
                        Towar skladnik = sgt.TowaryManager.WczytajTowar(skl.TowarId);

                        if (!validItems.Contains(skladnik.Symbol))
                        {
                            skl.Usun();
                        }
                    }

                    foreach (var item in p.Packages)
                    {
                        if (!sgt.TowaryManager.IstniejeWg(item.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                        {
                            return TypedResults.BadRequest($"Package [Id={item.Id}] can not be assigned, because it does not exit");
                        }

                        Towar s = sgt.TowaryManager.WczytajTowarWg(item.Id, TowarParamWyszukEnum.gtaTowarWgSymbolu);

                        bool found = false;

                        foreach (TwSkladnik skl in tw.Skladniki)
                        {
                            Towar skladnik = sgt.TowaryManager.WczytajTowar(skl.TowarId);

                            if (skladnik.Symbol == item.Id)
                            {
                                found = true;

                                if (skl.Ilosc != item.Quantity)
                                {
                                    skl.Ilosc = item.Quantity;
                                }
                            }
                        }

                        if (!found)
                        {
                            TwSkladnik skl = tw.Skladniki.Dodaj(s.Identyfikator);
                            skl.Ilosc = item.Quantity;
                        }
                    }
                }

                if (p.Image != null && p.Image != "")
                {
                    string tmpName = $"./tmp/{DateTime.Now:HH.mm.ss.ffff}";
                    string tmpJpg = tmpName + ".jpg";

                    byte[] enc;

                    try
                    {
                       enc = Convert.FromBase64String(p.Image);
                    }
                    catch(FormatException)
                    {
                        return TypedResults.BadRequest("Cannot read image");
                    }
                    
                    File.WriteAllBytes(tmpName, enc);

                    var im = (Bitmap)System.Drawing.Image.FromFile(tmpName);
                    im = Utils.Utils.ProcessImageThumbnail(im, goalAspectRatio: 1.0f);

                    Utils.Utils.SaveImgAsJpeg(tmpJpg, im);
                    im.Dispose();

                    if (tw.Zdjecia.Liczba <= 0)
                    {
                        TwZdjecie img = tw.Zdjecia.Dodaj(tmpJpg);
                        img.Glowne = true;
                    }
                    else
                    {
                        Dodatki dod = new();
                        TwZdjecie img = tw.Zdjecia.Wczytaj(1);
                        var z = dod.ZmienBinariaNaZdjecie(img.Zdjecie);

                        TwZdjecie imgNew = tw.Zdjecia.Dodaj(tmpJpg);

                        if (((byte[])img.Zdjecie).Length != ((byte[])imgNew.Zdjecie).Length)
                        {
                            imgNew.Glowne = true;
                            img.Usun();
                        }
                        else
                        {
                            imgNew.Usun();
                        }
                    }
                }

                tw.Zapisz();

                int id = tw.Identyfikator;
                tw.Zamknij();

                return TypedResults.Ok(id);
            });
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="code">Product identifier</param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async static Task<IResult> Delete(string code, SubiektGT worker)
        {
            return await worker.EnqueueAsync<IResult>(sgt =>
            {
                Towar tw;

                if (!sgt.TowaryManager.IstniejeWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu))
                {
                    return TypedResults.BadRequest("Product does not exist");
                }

                tw = sgt.TowaryManager.WczytajTowarWg(code, TowarParamWyszukEnum.gtaTowarWgSymbolu);
                
                if(tw.MoznaUsunac)
                {
                    tw.Usun();
                    return TypedResults.Ok();
                }

                return TypedResults.Conflict("Can not delete object");
            });
        }

        private static void ClearTempFiles()
        {
            string tmpdir = "./tmp";
            if (!Directory.Exists(tmpdir))
            {
                Directory.CreateDirectory(tmpdir);
            }
            else
            {
                foreach (var f in Directory.EnumerateFiles(tmpdir))
                {
                    File.Delete(f);
                }
            }
        }
    }
}
