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
            await db.Brands.AddRangeAsync(CreateBrands(random, 18));
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
                " - передовая компания по производству высококачественных кормов.",
                " - отличный производителей кормов.",
                " - только качествнный кормов.",
                " и точка.",
                " - популярный производитель кормов для животных."
            };
            var brands = new string[]
            {
                "Updog","Pisna","Ligma","Mulina","DeeDeeGree","Wikas",
                "Mulina","HoPs","Focus","Sima","Geliks","Oleg","WaNNA","Ololsef","Billy H. Foods",
                "Dobry", "Мираторг", "Ангарский мясокомбинат", "Gulina"
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
                "ProBalance", "Fit", "Yum", "Expert", "Sensetive", "Care", "Happy", "One",
                "Royal", "Fine"
            };
            var nameParts2 = new string[]
            {
                "", "", "", "", " Super", " Pro", " Elite"
            };
            var taste = new string[]
            {
                "Курица", "Индейка; курица; лосось", "Индейка", "Лосось", "Говядина", "Баранина", "Индейка; курица", "Курица; Говядина"
            };
            var specsCat = new string[]
            {
                "Для активных котов", "Для кастрированных питомцев котов", "Для котят", "Для взрослых", "Корм для похудения",
                "Обычный"
            };
            var specsDog = new string[]
            {
                "Для активных собак", "Для кастрированных собак", "Для щенков", "Для взрослых", "Корм для похудения",
                "Обычный"
            };

            return Enumerable.Range(0, count).Select(i =>
            {
                var rnd = random.Next(0, 18);
                var species = "";
                var spec = "";
                var tast = taste[random.Next(0, taste.Length)];
                var desc = "Сбалансированный корм. " +
                    "Состав корма: дегидратированное " + tast.ToLower() + " мин. 30 % (в т. ч. из курицы мин. 23 %), рис, ячмень, протеины.";
                if (rnd == 0)
                {
                    species = "Хомяк";
                    spec = "Обычный";
                }
                else if (rnd >= 1 && rnd <= 5)
                {
                    species = "Собака";
                    spec = specsDog[random.Next(0, specsDog.Length)];
                }
                else
                {
                    species = "Кот";
                    spec = specsCat[random.Next(0, specsCat.Length)];
                }

                return new Product
                {
                    BrandId = brandsGuid[random.Next(0, brandsGuid.Count)],
                    Name = nameParts1[random.Next(0, nameParts1.Length)] +
                        nameParts2[random.Next(0, nameParts2.Length)],
                    Photo1 = System.IO.File.ReadAllBytes($"{basePath}/FakerPictures/proj_{rnd}.jpg"),
                    Species = species,
                    Spec = spec,
                    Available = true,
                    Description = desc, 
                    Taste = tast,
                    WeightG = random.Next(5, 40) * 100,
                    PriceKopeck = random.Next(800, 4000) * 100,
                    Wet = random.Next(0, 4) != 2
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
    }
}
