using CatalogService.Controllers;
using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Dynamic;
using Xunit.Abstractions;

namespace CatalogService.Tests
{
    public class UnitTests
    {
        Database database;
        ProductsController prodController;
        BrandsController brandController;
        Random rnd = new Random(0);
        List<Brand> brands;
        List<Product> products;
        private readonly ITestOutputHelper output;
        int brandsCount = 18;

        public UnitTests(ITestOutputHelper output)
        {
            this.output = output;
            var options =
                new DbContextOptionsBuilder<Database>()
                    .UseNpgsql("Server=localhost:5532;Username=test;Password=test;Database=test")
                    .Options;

            database = new Database(options, delete: true);

            prodController = new ProductsController(database);
            brandController = new BrandsController(database);

            brands = FakerController.CreateBrands(rnd, brandsCount, "M:\\source\\ProjectorShop\\CatalogService").ToList();

            database.Brands.AddRange(brands);
            database.SaveChanges();
            brands = database.Brands.ToList();

            products = FakerController.CreateProducts(rnd, brands.Select(b => b.Id).ToList(), 100, "M:\\source\\ProjectorShop\\CatalogService").ToList();
            database.Products.AddRange(products);
            database.SaveChanges();
            products = database.Products.ToList();
        }

        [Fact]
        public async void Products_Index_First_Page()
        {
            var ret = await prodController.Index();
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(30, list.Count);

            for (int i = 0; i < 30; i++)
            {
                Assert.NotNull(list[i].Id);
                Assert.NotNull(list[i].Name);
                Assert.NotNull(list[i].Brand);
            }
        }

        [Fact]
        public async void Products_Index_Last_Page()
        {
            var ret = await prodController.Index(page: 4);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(10, list.Count);

            for (int i = 0; i < 10; i++)
            {
                Assert.NotNull(list[i].Id);
                Assert.NotNull(list[i].Name);
                Assert.NotNull(list[i].Brand);
            }
        }

        [Fact]
        public async void Products_Index_IdList()
        {
            var ret = await prodController.Index(idList: string.Join(",", products.Take(2).Select(x => x.Id.ToString())));
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async void Products_Index_Details()
        {
            var ret = await prodController.Index(id: (Guid)products[0].Id);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var obj = ToDynamic(dyn);

            Assert.Equal(products[0].Id, obj.Id);
        }

        [Fact]
        public async void Products_Index_ByBrand_Fail()
        {
            var ret = await prodController.ByBrand((Guid)brands[0].Id);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(4, list.Count);
        }

        [Fact]
        public async void Products_Index_ByBrand_Ok()
        {
            var ret = await prodController.ByBrand((Guid)brands[1].Id);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async void Products_Delete()
        {
            Assert.NotNull(database.Products.Where(x => x.Id == products[0].Id).FirstOrDefault());
            var ret = await prodController.Delete(id: (Guid)products[0].Id);
            Assert.IsType<OkResult>(ret);
            Assert.Null(database.Products.Where(x => x.Id == products[0].Id).FirstOrDefault());
        }

        [Fact]
        public async void Products_Edit()
        {
            var newGuid = Guid.NewGuid();
            var ret = await prodController.Edit(new Product()
            {
                Id = (Guid)products[0].Id,
                Spec = "TEST1",
                Species = "TEST2",
                Name = "TEST3",
                BrandId = newGuid
            });

            Assert.IsType<OkObjectResult>(ret);

            var changed = database.Products.Where(x => x.Id == products[0].Id).FirstOrDefault();

            Assert.NotNull(changed);
            Assert.Equal("TEST1", changed.Spec);
            Assert.Equal("TEST2", changed.Species);
            Assert.Equal("TEST3", changed.Name);
            Assert.Equal(newGuid, changed.BrandId);
        }

        [Fact]
        public async void Products_Create()
        {
            var newGuid = brands[0].Id;
            var ret = await prodController.Create(new Product()
            {
                Spec = "CTEST1",
                Species = "CTEST2",
                Name = "CTEST3",
                BrandId = newGuid
            });

            Assert.IsType<OkObjectResult>(ret);

            var created = database.Products.Where(x => x.Name == "CTEST3").FirstOrDefault();

            Assert.NotNull(created);
            Assert.Equal("CTEST1", created.Spec);
            Assert.Equal("CTEST2", created.Species);
            Assert.Equal(newGuid, created.BrandId);
        }

        [Fact]
        public async void Products_GetPhoto_Ok()
        {
            var ret = await prodController.GetPhoto(products[50].Id.Value);
            Assert.IsType<FileContentResult>(ret);
        }


        [Fact]
        public async void Brands_Index_First_Page()
        {
            var ret = await brandController.Details();
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(brandsCount, list.Count);

            for (int i = 0; i < brandsCount; i++)
            {
                Assert.NotNull(list[i].Id);
                Assert.NotNull(list[i].Name);
                Assert.NotNull(list[i].Country);
            }
        }

        [Fact]
        public async void Brands_Index_All()
        {
            var ret = await brandController.Index();
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var list = ToDynamicList(dyn);

            Assert.Equal(brandsCount, list.Count);

            for (int i = 0; i < brandsCount; i++)
            {
                Assert.NotNull(list[i].Id);
                Assert.NotNull(list[i].Name);
            }
        }

        [Fact]
        public async void Brands_Index_Details()
        {
            var b = brands[0];
            var ret = await brandController.Index(id: (Guid)b.Id);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var obj = ToDynamic(dyn);

            Assert.NotNull(obj);
            Assert.Equal(b.Country, obj.Country);
            Assert.Equal(b.Description, obj.Description);
            Assert.Equal(b.Email, obj.Email);
            Assert.Equal(b.Phone, obj.Phone);
            Assert.Equal(b.Website, obj.Website);
            Assert.Equal(b.Year, obj.Year);
        }

        [Fact]
        public async void Brands_Delete()
        {
            Assert.NotNull(database.Brands.Where(x => x.Id == brands[0].Id).FirstOrDefault());
            var ret = await brandController.Delete(id: (Guid)brands[0].Id);
            Assert.IsType<OkResult>(ret);
            Assert.Null(database.Brands.Where(x => x.Id == brands[0].Id).FirstOrDefault());
        }

        [Fact]
        public async void Brands_Edit()
        {
            var ret = await brandController.Edit(new Brand()
            {
                Id = (Guid)brands[0].Id,
                Country = "CTEST1",
                Description = "CTEST2",
                Email = "CTEST3",
                Phone = "CTEST4",
                Website = "CTEST5",
                Name = "CTEST6",
                Year = 2000
            });

            Assert.IsType<OkResult>(ret);

            var changed = database.Brands.Where(x => x.Id == brands[0].Id).FirstOrDefault();

            Assert.NotNull(changed);
            Assert.Equal("CTEST1", changed.Country);
            Assert.Equal("CTEST2", changed.Description);
            Assert.Equal("CTEST3", changed.Email);
            Assert.Equal("CTEST4", changed.Phone);
            Assert.Equal("CTEST5", changed.Website);
            Assert.Equal(2000, changed.Year);
        }

        [Fact]
        public async void Brands_Create()
        {
            var ret = await brandController.Create(new Brand()
            {
                Country = "CTEST1",
                Description = "CTEST2",
                Email = "CTEST3",
                Phone = "CTEST4",
                Website = "CTEST5",
                Name = "CTEST6",
                Year = 2000
            });

            Assert.IsType<OkObjectResult>(ret);

            var created = database.Brands.Where(x => x.Name == "CTEST6").FirstOrDefault();

            Assert.NotNull(created);
            Assert.Equal("CTEST1", created.Country);
            Assert.Equal("CTEST2", created.Description);
            Assert.Equal("CTEST3", created.Email);
            Assert.Equal("CTEST4", created.Phone);
            Assert.Equal("CTEST5", created.Website);
            Assert.Equal(2000, created.Year);
        }

        [Fact]
        public async void Brands_GetPhoto()
        {
            var ret = await brandController.GetLogo(brands[10].Id.Value);
            Assert.IsType<FileContentResult>(ret);
        }

        public static List<dynamic> ToDynamicList(dynamic value)
        {
            var list = new List<dynamic>();

            foreach (var item in value)
                list.Add(ToDynamic(item));

            return list;
        }

        public static dynamic ToDynamic(object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value) ?? "");

            return expando as ExpandoObject;
        }
    }
}