using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private Database db;

        public UsersController(Database db)
        {
            this.db = db;
        }

        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromBody] UserRegister userData)
        {
            try
            {
                var result = await db.Users
                    .Where(p => p.Login == userData.Login && p.PasswordHash == Utils.SHA512(userData.Password))
                    .Select(u => new { u.Id })
                    .FirstOrDefaultAsync();

                if (result == null)
                    return BadRequest();

                return Ok(AuthOptions.GetToken(result.Id, DateTime.Now));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetId")]
        public async Task<IActionResult> GetId()
        {
            try
            {
                if (User.Identity?.Name == null)
                    return Unauthorized();

                var result = await db.Users.FindAsync(Guid.Parse(User.Identity!.Name));

                if (result == null)
                    return Unauthorized();

                return Ok(result.Id.ToString());
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }

        [HttpGet("Me")]
        public async Task<IActionResult> Me(bool info = false)
        {
            try
            {
                if (User.Identity?.Name == null)
                    return Ok(new { Auth = false });

                var result = await db.Users.FindAsync(Guid.Parse(User.Identity!.Name));

                if (result == null)
                    return NotFound();

                if (info)
                    return Ok(result);
                else
                    return Ok(new { result.Login, result.IsAdmin, result.Id, Auth = true });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpGet("HaveAdminAccess")]
        public async Task<bool> HaveAdminAccess() => (await db.Users.FindAsync(Guid.Parse(User.Identity?.Name)))?.IsAdmin == true;

        private bool HasRightToModifyAccount(Guid? id, bool isAdmin = false) =>
            isAdmin || id.ToString() == User.Identity?.Name;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if ((await db.Users.FindAsync(User.Identity?.Name))?.IsAdmin == true)
                return Unauthorized();

            return Ok(await db.Users.ToListAsync());
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (!HasRightToModifyAccount(id, result?.IsAdmin == true))
                return Unauthorized();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] UserRegister user)
        {
            if (Environment.GetEnvironmentVariable("ALLOW_REGISTRATION") == "false")
                return BadRequest("В демо не разрешена регистрация");

            try
            {
                await db.Users.AddAsync(new User
                {
                    Login = user.Login,
                    PasswordHash = Utils.SHA512(user.Password ?? "")
                });
                await db.SaveChangesAsync();

                return Ok();
            } catch (Exception) { }

            return BadRequest();
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit([FromBody] User user)
        {
            var result = await db.Users.Where(p => p.Id == user.Id).FirstOrDefaultAsync();

            if (!HasRightToModifyAccount(user.Id, result.IsAdmin == true))
                return Unauthorized();

            if (result == null)
                return NotFound();

            try
            {
                result.Email = user.Email;
                result.Address = user.Address;
                result.Lastname = user.Lastname;
                result.Middlename = user.Middlename;
                result.Firstname = user.Firstname;
                result.Phone = user.Phone;
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }

        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserRegister user)
        {
            if (User.Identity?.Name == null)
                return Unauthorized();

            var result = await db.Users.FindAsync(Guid.Parse(User.Identity?.Name));

            if (result == null)
                return NotFound();

            try
            {
                result.PasswordHash = Utils.SHA512(user.Password);
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception) { }

            return BadRequest();
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (!HasRightToModifyAccount(id, result.IsAdmin))
                return Unauthorized();

            if (result == null)
                return NotFound();

            db.Users.Remove(result);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}