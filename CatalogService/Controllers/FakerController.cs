using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.Json;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.Fonts;
using FontStyle = SixLabors.Fonts.FontStyle;
using Image = SixLabors.ImageSharp.Image;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FakerController: ControllerBase
    {
        private Database db;
        private Random random;

        public FakerController(Database db)
        {
            this.db = db;
            random = new Random();
        }

        [HttpGet("FakeProducts")]
        public async Task<IActionResult> FakeProducts()
        {
            await db.Products.AddRangeAsync(CreateProducts(random, db.Brands.Select(b => b.Id).ToList(), 100));
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("FakeBrands")]
        public async Task<IActionResult> FakeBrands()
        {
            await db.Brands.AddRangeAsync(CreateBrands(random, 100));
            await db.SaveChangesAsync();
            return Ok();
        }

        public static IEnumerable<Brand> CreateBrands(Random random, int count, string basePath = ".")
        {
            var countries = new string[]
            {
                "Россия", "Беларусь", "США", "Украина",
                "Китай", "Тайвань", "Германия", "Франция"
            };
            var descriptions = new string[]
            {
                " - передовая компания по производству проекторов.",
                " - отличный производителей проекторов.",
                " - только качественное оборудование.",
                " и точка.",
                " - популярный производитель техники."
            };
            var brands = new string[]
            {
                "Acer","BenQ","Cactus","Digma","Ligma","Epson",
                "HIPER","HP","InFocus","Philips","Rombica","Samsung","ViewSonic","XGIMI","Xiaomi","ZDK",
                "BrightLight","KingProj","Skoofedone","POOOp","Bebop","Retjio","WeeWee","BabbyJohn","Amigus",
                "Dio","posOS","Qebra","Bebra","Billy Herrington Electronics","Gachi&Bindera",
                "Kifeo","Lui","Boombassia","Tompson","Irain","Pisna","Updog","Joe","Candice",
                "Reihka","Asiss","Zela","SYMFONIA","Leg","Oleg","WaNNA","NoGI","Ololsef","Hersojo",
                "Uqrope","Ilyui","Debold","Rass","OSD","COK","Hoyux","Oxana","Nvord","iBLOON",
                "Yijoba","Viktor","Japanese Image Company","Oi","PQ","Uert","Pops","Iver",
                "Doos","Ooof","Rasa","Amenba","Voas","Uzad","Kahae","Rfowe","Adaop","Idap",
                "Napoe","Uaso","Rasop","Ahodas","Tafidop","Rasov","Osas", "Ssdsd","Slpoe","Ietc",
                "Ypwo","SLP","KASA","AFL","YPP","AWW","Vivitek","OPTOMA","Sima","HYRR","LDCCV","ACS"
            };

            return Enumerable.Range(0, count).Select(i =>
            {
                var website = brands[i].ToLower().Replace(" ", "");
                return new Brand
                {
                    Country = countries[random.Next(0, countries.Length)],
                    Description = brands[i] + descriptions[random.Next(0, descriptions.Length)],
                    Email = $"support@{website}.com",
                    Logo = CreateImage(random, brands[i], basePath),
                    Name = brands[i],
                    Phone = $"+7 (800) {random.Next(100, 999)} {random.Next(10, 99)}-{random.Next(10, 99)}",
                    Website = $"https://{website}.com",
                    Year = random.Next(1980, 2022)
                };
            });
        }

        public static IEnumerable<Product> CreateProducts(Random random, List<Guid?> brandsGuid, int count, string basePath = ".")
        {
            var nameParts1 = new string[]
            {
                "WE", "X", "M", "LI"
            };
            var nameParts2 = new string[]
            {
                "", "", "", " Progec", " Super", " Pro", " Elite"
            };

            return Enumerable.Range(0, count).Select(i =>
            {
                return new Product
                {
                    BrandId = brandsGuid[random.Next(0, count + 1)],
                    Color = "Черный",
                    Model = nameParts1[random.Next(0, nameParts1.Length)] +
                        random.Next(0, 158) +
                        random.Next(0, 745) +
                        nameParts1[random.Next(0, nameParts1.Length)] +
                        random.Next(0, 874) +
                        nameParts2[random.Next(0, nameParts2.Length)],
                    Photo1 = System.IO.File.ReadAllBytes($"{basePath}/FakerPictures/proj_{random.Next(0, 18)}.jpg"),
                    Resolution = "840x480",
                    OtherSpecs = new Dictionary<string, string> {
                        { "ProjectionTechnology", "LCD" },
                        { "HDR", "Нет" },
                        { "LampType", "LED" }
                    }
                };
            });
        }

        public static byte[] CreateImage(Random random, string text, string basePath)
        {
            var collection = new FontCollection();
            var family = collection.Add($"{basePath}/Assets/font.ttf");
            var font = family.CreateFont(56, FontStyle.Italic);
            var color = Color.FromRgb((byte)random.Next(64, 256), (byte)random.Next(64, 256), (byte)random.Next(64, 256));
            
            using (Image<Rgba32> image = new(256, 96))
            {
                image.Mutate(x => x.Fill(color));
                image.Mutate(x => x.DrawText(text, font, Color.Black, new PointF(16, 20)));
                using var ms = new MemoryStream();
                image.SaveAsJpeg(ms);
                return ms.ToArray();
            }
        }

        /*public static byte[] CreateBoxImage(Image image)
        {
            using (Image template = Image.Load("Assets/BoxTemplate.png"))
            {
                template.Mutate(x => x.DrawImage(image, 1));
                using var ms = new MemoryStream();
                template.SaveAsJpeg(ms);
                return ms.ToArray();
            }
        }*/
    }
}
