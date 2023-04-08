using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }
        public string? Model { get; set; }
        public Brand? Brand { get; set; }
        public Guid? BrandId { get; set; }
        public byte[]? Photo1 { get; set; }
        public byte[]? Photo2 { get; set; }
        public double Price { get; set; }
        public string? Resolution { get; set; }
        public string? Color { get; set; }
        public Dictionary<string, string>? OtherSpecs { get; set; }
    }
}