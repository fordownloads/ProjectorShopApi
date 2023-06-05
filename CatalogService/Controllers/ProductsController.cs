using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private Database db;

        public ProductsController(Database db)
        {
            this.db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index
        (
            [FromQuery] int page = 1, 
            [FromQuery] string idList = "null", 
            [FromQuery] string species = "null",
            [FromQuery] int limit = 30
        )
        {
            var idListList = idList == "null" ? new List<Guid>() : idList.Split(',').Select(x => Guid.Parse(x)).ToList();
            if (idList != "null")
                limit = 10000;
            var offset = limit * (page - 1);

            var where = idList == "null" ? db.Products : db.Products.Where(x => idListList.Contains((Guid)x.Id));

            if (species != "null")
                where = where.Where(x => x.Species == species);

            var result = await where
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Brand)
                .Select(p => new
                    {
                        p.Id, p.Name, p.PriceKopeck, p.Species, p.Available, p.WeightG,
                        Brand = new
                        {
                            p.Brand.Name,
                            p.Brand.Id
                        }
                    }
                ).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var result = await db.Products
                .Where(p => p.Id == id).Include(p => p.Brand)
                .Select(p => new {  p.Id,  p.Name,  p.Spec, p.Species, p.Available, p.PriceKopeck, p.Description, p.Taste, p.WeightG, p.Wet, Brand = new { p.Brand.Name, p.Brand.Id } }
                ).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("ByBrand/{id}")]
        public async Task<IActionResult> ByBrand(Guid id)
        {
            var result = await db.Products
                .Where(p => p.BrandId == id)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.PriceKopeck,
                    p.Species,
                    p.Available,
                    p.WeightG,
                    Brand = new { p.Brand.Name, p.Brand.Id } }
                ).ToArrayAsync();

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("Image/{id}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 6000)]
        public async Task<IActionResult> GetPhoto(Guid id)
        {
            var product = db.Products
                .Where(p => p.Id == id);

            try
            {
                var photo = await product.Select(p => p.Photo1).FirstOrDefaultAsync();
                if (photo == null)
                    return NotFound();

                return File(photo, "image/jpeg");
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                product.Id = null;
                await db.Products.AddAsync(product);
                await db.SaveChangesAsync();

                return Ok(product);
            } catch (Exception e) { return BadRequest(e.Message); }

            
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit([FromBody] Product product)
        {
            var result = await db.Products.FindAsync(product.Id.Value);

            if (result == null)
                return NotFound();

            try
            {
                result.Spec = product.Spec;
                result.Species = product.Species;
                result.BrandId = product.BrandId;
                result.Name = product.Name;
                result.PriceKopeck = product.PriceKopeck;
                result.Wet = product.Wet;
                result.Taste = product.Taste;
                result.Available = product.Available;
                result.Description = product.Description;
                result.WeightG = product.WeightG;
                await db.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception e) { return Ok(e.Message); }            
        }

        public static async Task<byte[]> ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        [HttpPost("EditPhoto/{id}")]
        public async Task<IActionResult> EditPhotos(Guid id)
        {
            var result = await db.Products.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            var photo = await ReadFully(Request.Body);

            try
            {
                result.Photo1 = photo;
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception) { }

            return BadRequest();
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await db.Products.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            db.Products.Remove(result);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}