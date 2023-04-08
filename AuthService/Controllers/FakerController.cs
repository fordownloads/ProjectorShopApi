using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.Json;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FakerController: ControllerBase
    {
        static public readonly User DefaultAdmin = new User { Login = "admin228", PasswordHash = Utils.SHA512("MrBebra1337$ex"), IsAdmin = true };

        private Database db;

        public FakerController(Database db)
        {
            this.db = db;
        }

        [HttpGet("FillData")]
        public async Task<IActionResult> FillData()
        {
            await db.Users.AddAsync(DefaultAdmin);
            await db.SaveChangesAsync();
            return Ok();
        }
    }
}
