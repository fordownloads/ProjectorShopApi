using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Brand? Brand { get; set; }
        public Guid? BrandId { get; set; }
        public byte[]? Photo1 { get; set; }
        public bool Available { get; set; } = true;
        public bool Wet { get; set; } = true;
        public int PriceKopeck { get; set; }
        public string Species { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public string Taste { get; set; } = string.Empty;
        public int WeightG { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}