using AuthService.Controllers;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService
{
    public class Database : DbContext
    {
        public DbSet<User> Users { get; set; }

        public Database(DbContextOptions<Database> o, bool delete = false) : base(o)
        {
            if (delete)
                Database.EnsureDeleted();
            Database.EnsureCreated();

            if (Users?.FirstOrDefault(x => x.IsAdmin == true) == null)
            {
                Users.Add(FakerController.DefaultAdmin);
                SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>(e => e.HasKey(k => k.Id));
        }
    }
}
