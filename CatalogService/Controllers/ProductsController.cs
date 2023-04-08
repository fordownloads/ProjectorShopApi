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
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string idList = "null")
        {
            var idListList = idList == "null" ? new List<Guid>() : idList.Split(',').Select(x => Guid.Parse(x)).ToList();
            var limit = idList == "null" ? 30 : 10000;
            var offset = limit * (page - 1);

            var where = idList == "null" ? db.Products : db.Products.Where(x => idListList.Contains((Guid)x.Id));

            var result = await where
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Brand)
                .Select(p => new
                    {
                        p.Id, p.Model, p.Color, p.Resolution,
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
                .Select(p => new {  p.Id,  p.Model,  p.Color, p.Resolution,
                        p.OtherSpecs, Brand = new { p.Brand.Name, p.Brand.Id } }
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
                .Select(p => new { p.Id,  p.Model,  p.Color, p.Resolution,
                        Brand = new { p.Brand.Name, p.Brand.Id } }
                ).ToArrayAsync();

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("Image/{id}/{photoId}")]
        public async Task<IActionResult> GetPhoto(Guid id, int photoId)
        {
            var product = db.Products
                .Where(p => p.Id == id);

            try
            {
                var photo = await (photoId switch
                {
                    0 => product.Select(p => p.Photo1),
                    1 => product.Select(p => p.Photo2),
                    _ => throw new ArgumentException()
                }).FirstOrDefaultAsync();
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
                result.Color = product.Color;
                result.Resolution = product.Resolution;
                result.BrandId = product.BrandId;
                result.Model = product.Model;
                result.Price = product.Price;
                //result.OtherSpecs = product.OtherSpecs;
                await db.SaveChangesAsync();

                return Ok();
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

        [HttpPost("EditPhoto/{id}/{photoId}")]
        public async Task<IActionResult> EditPhotos(Guid id, int photoId)
        {
            var result = await db.Products.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            var photo = await ReadFully(Request.Body);

            try
            {
                switch (photoId)
                {
                    case 0:
                        result.Photo1 = photo;
                        break;
                    case 1:
                        result.Photo2 = photo;
                        break;
                    default:
                        return BadRequest("Invalid id");
                }
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