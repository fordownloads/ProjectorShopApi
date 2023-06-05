using CatalogService.Controllers;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService
{
    public class Database : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }

        public Database(DbContextOptions<Database> o, bool delete = false) : base(o)
        {
            if (delete)
                Database.EnsureDeleted();
            Database.EnsureCreated();

            if (Environment.GetEnvironmentVariable("ALLOW_AUTOFILL_DB") == "true")
            {
                if (Brands?.FirstOrDefault() == null)
                {
                    var b = FakerController.CreateBrands(Random.Shared, 18);
                    Brands.AddRange(b);
                    SaveChanges();
                }

                if (Products?.FirstOrDefault() == null)
                {
                    var p = FakerController.CreateProducts(Random.Shared, Brands.Take(18).Select(x => x.Id).ToList(), 100);
                    Products.AddRange(p);
                    SaveChanges();
                }
            }
    }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Product>(e => e.HasKey(k => k.Id));
            b.Entity<Brand>(e => e.HasKey(k => k.Id));
        }
    }
}
