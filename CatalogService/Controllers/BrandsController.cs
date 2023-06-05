using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BrandsController : ControllerBase
    {
        private Database db;

        public BrandsController(Database db)
        {
            this.db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return Ok(await db.Brands.Select(b => new { b.Id, b.Name }).ToListAsync());
        }

        [HttpGet("Details")]
        public async Task<IActionResult> Details(int page = 1)
        {
            var limit = 30;
            var offset = limit * (page - 1);
            return Ok(await db.Brands.Skip(offset).Take(limit).Select(b => new { b.Id, b.Name, b.Country, b.Email, b.Phone, b.Website }).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var result = await db.Brands
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id, p.Country, p.Website, p.Description, p.Year, p.Email, p.Phone, p.Name
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("Image/{id}")]
        public async Task<IActionResult> GetLogo(Guid id)
        {
            var logo = await db.Brands
                .Where(b => b.Id == id)
                .Select(b => b.Logo)
                .FirstOrDefaultAsync();

            if (logo == null)
                return NotFound();

            return File(logo, "image/jpeg");
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Brand brand)
        {
            try
            {
                brand.Id = null;
                await db.Brands.AddAsync(brand);
                await db.SaveChangesAsync();

                return Ok(brand);
            }
            catch (Exception) { }

            return BadRequest();
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit([FromBody] Brand brand)
        {
            var result = await db.Brands.Where(p => p.Id == brand.Id).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            try
            {
                result.Website = brand.Website;
                result.Country = brand.Country;
                result.Name = brand.Name;
                result.Email = brand.Email;
                result.Phone = brand.Phone;
                result.Year = brand.Year;
                result.Description = brand.Description;
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception) { }

            return BadRequest();
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

        [HttpPost("EditLogo/{id}")]
        public async Task<IActionResult> EditLogo(Guid id)
        {
            var result = await db.Brands.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            var logo = await ReadFully(Request.Body);

            try
            {
                result.Logo = logo.ToArray();
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception) { }

            return BadRequest();
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await db.Brands.FindAsync(id);

            if (result == null)
                return NotFound();

            db.Brands.Remove(result);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}