using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace SGT_BRIDGE.Utils
{
    public class Utils
    {
        public static readonly List<string> COUNTRY_ISO_2 = new List<string>() { "AF", "AL", "DZ", "AD", "AO", "AI", "AQ", "AG", "SA", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB", "BE", "BZ", "BJ", "BM", "BT", "BY", "BO", "BQ", "BA", "BW", "BR", "BN", "IO", "VG", "BG", "BF", "BI", "CL", "CN", "HR", "CW", "CY", "TD", "ME", "CZ", "UM", "DK", "CD", "DM", "DO", "DJ", "EG", "EC", "ER", "EE", "SZ", "ET", "FK", "FJ", "PH", "FI", "FR", "TF", "GA", "GM", "GS", "GH", "GI", "GR", "GD", "GL", "GE", "GU", "GG", "GF", "GY", "GP", "GT", "GW", "GQ", "GN", "HT", "ES", "NL", "HN", "HK", "IN", "ID", "IQ", "IR", "IE", "IS", "IL", "JM", "JP", "YE", "JE", "JO", "KY", "KH", "CM", "CA", "QA", "KZ", "KE", "KG", "KI", "CO", "KM", "CG", "KR", "KP", "CR", "CU", "KW", "LA", "LS", "LB", "LR", "LY", "LI", "LT", "LU", "LV", "MK", "MG", "YT", "MO", "MW", "MV", "MY", "ML", "MT", "MP", "MA", "MQ", "MR", "MU", "MX", "FM", "MM", "MD", "MC", "MN", "MS", "MZ", "NA", "NR", "NP", "DE", "NE", "NG", "NI", "NU", "NF", "NO", "NC", "NZ", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PN", "PF", "PL", "PR", "PT", "ZA", "CF", "CV", "RE", "RU", "RO", "RW", "EH", "KN", "LC", "VC", "BL", "MF", "PM", "SV", "AS", "WS", "SM", "SN", "RS", "SC", "SL", "SG", "SX", "SK", "SI", "SO", "LK", "US", "SD", "SS", "SR", "SJ", "SY", "CH", "SE", "TJ", "TH", "TW", "TZ", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "UY", "UZ", "VU", "WF", "VA", "VE", "HU", "GB", "VN", "IT", "CI", "BV", "CX", "IM", "SH", "AX", "CK", "VI", "HM", "CC", "MH", "FO", "SB", "ST", "ZM", "ZW", "AE" };

        public static readonly List<string> COUNTRY_ISO_3 = new List<string>() { "AFG", "ALB", "DZA", "AND", "AGO", "AIA", "ATA", "ATG", "SAU", "ARG", "ARM", "ABW", "AUS", "AUT", "AZE", "BHS", "BHR", "BGD", "BRB", "BEL", "BLZ", "BEN", "BMU", "BTN", "BLR", "BOL", "BES", "BIH", "BWA", "BRA", "BRN", "IOT", "VGB", "BGR", "BFA", "BDI", "CHL", "CHN", "HRV", "CUW", "CYP", "TCD", "MNE", "CZE", "UMI", "DNK", "COD", "DMA", "DOM", "DJI", "EGY", "ECU", "ERI", "EST", "SWZ", "ETH", "FLK", "FJI", "PHL", "FIN", "FRA", "ATF", "GAB", "GMB", "SGS", "GHA", "GIB", "GRC", "GRD", "GRL", "GEO", "GUM", "GGY", "GUF", "GUY", "GLP", "GTM", "GNB", "GNQ", "GIN", "HTI", "ESP", "NLD", "HND", "HKG", "IND", "IDN", "IRQ", "IRN", "IRL", "ISL", "ISR", "JAM", "JPN", "YEM", "JEY", "JOR", "CYM", "KHM", "CMR", "CAN", "QAT", "KAZ", "KEN", "KGZ", "KIR", "COL", "COM", "COG", "KOR", "PRK", "CRI", "CUB", "KWT", "LAO", "LSO", "LBN", "LBR", "LBY", "LIE", "LTU", "LUX", "LVA", "MKD", "MDG", "MYT", "MAC", "MWI", "MDV", "MYS", "MLI", "MLT", "MNP", "MAR", "MTQ", "MRT", "MUS", "MEX", "FSM", "MMR", "MDA", "MCO", "MNG", "MSR", "MOZ", "NAM", "NRU", "NPL", "DEU", "NER", "NGA", "NIC", "NIU", "NFK", "NOR", "NCL", "NZL", "OMN", "PAK", "PLW", "PSE", "PAN", "PNG", "PRY", "PER", "PCN", "PYF", "POL", "PRI", "PRT", "ZAF", "CAF", "CPV", "REU", "RUS", "ROU", "RWA", "ESH", "KNA", "LCA", "VCT", "BLM", "MAF", "SPM", "SLV", "ASM", "WSM", "SMR", "SEN", "SRB", "SYC", "SLE", "SGP", "SXM", "SVK", "SVN", "SOM", "LKA", "USA", "SDN", "SSD", "SUR", "SJM", "SYR", "CHE", "SWE", "TJK", "THA", "TWN", "TZA", "TLS", "TGO", "TKL", "TON", "TTO", "TUN", "TUR", "TKM", "TCA", "TUV", "UGA", "UKR", "URY", "UZB", "VUT", "WLF", "VAT", "VEN", "HUN", "GBR", "VNM", "ITA", "CIV", "BVT", "CXR", "IMN", "SHN", "ALA", "COK", "VIR", "HMD", "CCK", "MHL", "FRO", "SLB", "STP", "ZMB", "ZWE", "ARE" };

        public static readonly List<string> CURRENCY_ISO = new List<string>() { "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BOV", "BRL", "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHE", "CHF", "CHW", "CLF", "CLP", "COP", "COU", "CRC", "CUC", "CUP", "CVE", "CZK", "DJF", "DKK", "DOP", "DZD", "EGP", "ERN", "ETB", "EUR", "FJD", "FKP", "GBP", "GEL", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL", "HTG", "HUF", "IDR", "ILS", "INR", "IQD", "IRR", "ISK", "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD", "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL", "MGA", "MKD", "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN", "MXV", "MYR", "MZN", "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR", "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD", "CNY", "RUB", "RWF", "SAR", "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLE", "SLL", "SOS", "SRD", "SSP", "STN", "SVC", "SYP", "SZL", "THB", "TJS", "TMT", "TND", "TOP", "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "USN", "UYI", "UYU", "UYW", "UZS", "VED", "VES", "VND", "VUV", "WST", "XAF", "XAG", "XAU", "XBA", "XBB", "XBC", "XBD", "XCD", "XDR", "XOF", "XPD", "XPF", "XPT", "XSU", "XTS", "XUA", "XXX", "YER", "ZAR", "ZMW", "ZWL" };

        public static readonly Regex CITY_REGEX = new Regex(@"^([a-zA-ZzżźćńółęąśŻŹĆĄŚĘŁÓŃ]+|[a-zA-Z-zżźćńółęąśŻŹĆĄŚĘŁÓŃ]+\s[a-zA-ZzżźćńółęąśŻŹĆĄŚĘŁÓŃ]+)$");

        /// <summary>
        /// Make white bg thumbnail of image by follow steps:
        /// <para>1. Extract item bounding box</para>
        /// <para>2. Add padding to bounding box</para>
        /// <para>3. Add additional padding to reach goal aspect ratio</para>
        /// </summary>
        public static Bitmap ProcessImageThumbnail(Bitmap img, int padding = 50, float goalAspectRatio = 1.59f)
        {
            img = RemoveBackground(img);
            img = AddPadding(img, padding);
            img = AdjustToGoalAspect(img, goalAspectRatio);
            img = TransparencyToWhite(img);

            return img;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        public static string GetJpegBase64EncodedImage(string fname, int maxFileSize = 2)
        {
            string jpgFileName = fname.ToUpper().Replace(".PNG", ".JPG");
            Image img = Image.FromFile(fname, true);

            for (int i = 100; i > 5; i -= 5)
            {
                SaveImgAsJpeg(jpgFileName, img, i);
                FileInfo jpgDetails = new(jpgFileName);

                if (jpgDetails.Length <= maxFileSize * 1024 * 1024)
                {
                    break;
                }
            }

            byte[] imgBytes = File.ReadAllBytes(jpgFileName);
            string encodedImg = Convert.ToBase64String(imgBytes);

            File.Delete(jpgFileName);

            return "data:" + encodedImg;
        }

        #region filters

        public static Bitmap ToFullHD(Bitmap img)
        {
            if (img.Width >= 1920)
                return img;
            
            Bitmap newBitmap = new(1920, 1080);
            Graphics newGraphics = Graphics.FromImage(newBitmap);
            newGraphics.CompositingQuality = CompositingQuality.HighQuality;
            newGraphics.PageUnit = GraphicsUnit.Document;

            int x = (1920 - img.Width) / 2;
            int y = (1080 - img.Height) / 2;

            newGraphics.DrawImage(img, x, y);
            newGraphics.Dispose();

            return newBitmap;
        }

        public static Bitmap AddPadding(Bitmap img, int padding)
        {
            Bitmap tempBitmap = img;
            Bitmap newBitmap = new(tempBitmap.Width + 2 * padding, tempBitmap.Height + 2 * padding);
            Graphics newGraphics = Graphics.FromImage(newBitmap);
            newGraphics.DrawImage(img, padding, padding);

            newGraphics.Dispose();
            return newBitmap;
        }

        public static Bitmap AdjustBrightness(Bitmap Image, int Value)
        {
            Bitmap TempBitmap = Image;

            float FinalValue = (float)Value / 255.0f;

            Bitmap NewBitmap = new(TempBitmap.Width, TempBitmap.Height);

            Graphics NewGraphics = Graphics.FromImage(NewBitmap);

            float[][] FloatColorMatrix = [
                [1,0,0,0,0],
                [0,1,0,0,0],
                [0,0,1,0,0],
                [0,0,0,1,0],
                [FinalValue, FinalValue, FinalValue, 1, 1]

            ];
            System.Drawing.Imaging.ColorMatrix NewColorMatrix = new(FloatColorMatrix);

            System.Drawing.Imaging.ImageAttributes Attributes = new();

            Attributes.SetColorMatrix(NewColorMatrix);

            NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, GraphicsUnit.Pixel, Attributes);

            Attributes.Dispose();

            NewGraphics.Dispose();

            return NewBitmap;

        }

        public static Bitmap AdjustToGoalAspect(Bitmap img, float alfa_d)
        {
            float alfa = (float)img.Width / (float)img.Height;

            int hp = 0;
            int wp = 0;

            if (alfa >= alfa_d)
            {
                hp = (int)(0.5f * (img.Width * (1 / alfa_d) - img.Height));
            }
            else
            {
                wp = (int)(0.5f * (img.Height * alfa_d - img.Width));
            }

            Bitmap tempBitmap = img;
            Bitmap newBitmap;
            Graphics newGraphics;

            if (hp > 0)
            {
                newBitmap = new Bitmap(tempBitmap.Width, tempBitmap.Height + 2 * hp);
                newGraphics = Graphics.FromImage(newBitmap);
                newGraphics.DrawImage(img, 0, hp);
            }
            else if (wp > 0)
            {
                newBitmap = new Bitmap(tempBitmap.Width + 2 * wp, tempBitmap.Height);
                newGraphics = Graphics.FromImage(newBitmap);
                newGraphics.DrawImage(img, wp, 0);
            }
            else
            {
                return img;
            }

            if (newGraphics == null)
            {
                ;
            }
            else
            {
                newGraphics.Dispose();
            }

            return newBitmap;
        }

        public static Bitmap RemoveBackground(Bitmap img)
        {
            int[] bbox = GetBBox(img);
            int bboxWidth = bbox[2] - bbox[0];
            int bboxHeight = bbox[3] - bbox[1];

            Bitmap newBitmap = new(bboxWidth, bboxHeight);
            Graphics newGraphics = Graphics.FromImage(newBitmap);

            Rectangle cropRect = new(bbox[0], bbox[1], bboxWidth, bboxHeight);
            newGraphics.DrawImage(img, new Rectangle(0, 0, bboxWidth, bboxHeight), cropRect, GraphicsUnit.Pixel);
            newGraphics.Dispose();
            return newBitmap;
        }

        public static Bitmap TransparencyToWhite(Bitmap img)
        {
            Bitmap tempBitmap = img;
            Bitmap newBitmap = new(tempBitmap.Width, tempBitmap.Height);
            Graphics newGraphics = Graphics.FromImage(newBitmap);

            Color bgColor = Color.FromArgb(255, 255, 255, 255);

            ColorMap[] colorMap =
            [
                new()
                {
                    OldColor = Color.FromArgb(0, 0, 0, 0),
                    NewColor = bgColor
                },
                new()
                {
                    OldColor = Color.FromArgb(0, 0, 0, 255),
                    NewColor = bgColor
                },
            ];
            ImageAttributes attr = new();
            attr.SetRemapTable(colorMap);

            Rectangle rect = new(0, 0, img.Width, img.Height);

            newGraphics.DrawRectangle(new Pen(new SolidBrush(Color.White)), 0, 0, tempBitmap.Width, tempBitmap.Height);
            newGraphics.DrawImage(img, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, attr);

            newGraphics.Dispose();


            return newBitmap;
        }
        #endregion

        #region additional utils

        public static void SaveImgAsJpeg(string path, Image img, int quality = 100)
        {
            if (quality < 0 || quality > 100)
            {
                ArgumentOutOfRangeException ex = new("Image quality must be in range <0; 100>");
                throw ex;
            }

            EncoderParameter qualityParameter = new(System.Drawing.Imaging.Encoder.Quality, quality);
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new(1);
            encoderParams.Param[0] = qualityParameter;
            img.Save(path, jpegCodec, encoderParams);
            img.Dispose();
        }

        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType == mimeType)
                {
                    return codecs[i];
                }
            }

            throw new ArgumentException("Can not find encoder info for specified mimeType");
        }

        public static bool IsBg(Color c, Color bg)
        {
            if (c == bg)
                return true;
            return false;
        }

        public static int[] GetBBox(Bitmap img)
        {
            Color c;
            Color cWhite = img.GetPixel(2, 2);

            int yMin = 0;
            int yMax = img.Height;
            int xMin = 0;
            int xMax = img.Width;

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    c = img.GetPixel(x, y);

                    if (!IsBg(c, cWhite))
                    {
                        yMin = y;
                        goto endMinY;
                    }
                }
            }
        endMinY:

            for (int y = img.Height - 1; y > 0; y--)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    c = img.GetPixel(x, y);
                    if (!IsBg(c, cWhite))
                    {
                        yMax = y;
                        goto endMaxY;
                    }
                }
            }
        endMaxY:

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    c = img.GetPixel(x, y);
                    if (!IsBg(c, cWhite))
                    {
                        xMin = x;
                        goto endMinX;
                    }
                }
            }
        endMinX:

            for (int x = img.Width - 1; x > 0; x--)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    c = img.GetPixel(x, y);
                    if (!IsBg(c, cWhite))
                    {
                        xMax = x;
                        goto endMaxX;
                    }
                }
            }
        endMaxX:
            ;

            int[] ret = [xMin, yMin, xMax, yMax];
            return ret;
        }

        #endregion

    }
}
