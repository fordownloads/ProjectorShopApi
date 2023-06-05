using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using static APIGateway.Models;

namespace APIGateway.Controllers
{
    public class Utils
    {
        public static HttpClient client = new();
    }

    [ApiController]
    [Route("api")]
    public class GatewayController : ControllerBase
    {
        public GatewayController() { }

        private async Task<IActionResult> MicroServiceRequest(string path)
        {
            var r = await Utils.client.GetAsync("http://" + path);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                try
                {
                    return File(await r.Content.ReadAsStreamAsync(), r.Content.Headers.GetValues("Content-Type").First());
                }
                catch (InvalidOperationException e)
                {
                    return BadRequest();
                }
            return StatusCode((int)code);
        }

        private async Task<string?> GetUserId(HttpRequest request)
        {
            string? auth;
            try
            {
                auth = request.Cookies["token"];
            }
            catch (Exception)
            {
                return null;
            }

            if (auth == null)
                return null;

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://authservice/users/getid");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth);

            var r = await Utils.client.SendAsync(requestMessage);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                return await r.Content.ReadAsStringAsync();
            return null;
        }

        private async Task<bool> Authorized(HttpRequest request, Guid? idToCheck = null)
        {
            string? auth;
            try
            {
                auth = request.Cookies["token"];
            }
            catch (Exception)
            {
                return false;
            }

            if (auth == null)
                return false;

            var path = idToCheck switch
            {
                null => "http://authservice/users/haveadminaccess",
                _ => $"http://authservice/users/HasRightToModifyAccount?id={idToCheck}"
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth);

            var r = await Utils.client.SendAsync(requestMessage);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                return await r.Content.ReadAsStringAsync() == "true";
            return false;
        }

        private async Task<IActionResult> MicroServiceRequest(string path, object obj, bool json = true)
        {
            HttpContent byteContent;
            if (json)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
                byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                var photo = new MemoryStream();
                await ((IFormFile)obj).CopyToAsync(photo);
                byteContent = new ByteArrayContent(photo.ToArray());
            }

            var r = await Utils.client.PostAsync("http://" + path, byteContent);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                try
                {
                    if (json)
                        return File(await r.Content.ReadAsStreamAsync(), r.Content.Headers.ContentType?.MediaType ?? "application/json");
                    else
                        return Ok();
                }
                catch (InvalidOperationException e)
                {
                    return BadRequest("Gateway Error: " + e.Message);
                }
            return StatusCode((int)code);
        }

        [HttpPost("Users/Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return Ok();
        }

        [HttpPost("Users/GetToken")]
        public async Task<IActionResult> UGetToken([FromBody] UserRegister userData)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userData));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var r = await Utils.client.PostAsync("http://authservice/users/gettoken", byteContent);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                try
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        IsEssential = true
                    };
                    Response.Cookies.Append("token", await r.Content.ReadAsStringAsync(), options);
                    return Ok();
                }
                catch (InvalidOperationException)
                {
                    return BadRequest();
                }
            return StatusCode((int)code);
        }

        private async Task<IActionResult> MSRAuth(string path, object obj)
        {
            string? auth;
            try
            {
                auth = Request.Cookies["token"];
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            if (auth == null)
                return Unauthorized();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://" + path);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth);
            requestMessage.Content = byteContent;

            var r = await Utils.client.SendAsync(requestMessage);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                try
                {
                    return File(await r.Content.ReadAsStreamAsync(), r.Content.Headers.GetValues("Content-Type").First());
                }
                catch (InvalidOperationException)
                {
                    return BadRequest();
                }
            return StatusCode((int)code);
        }

        [HttpPost("Users/Me")]
        public async Task<IActionResult> UMe(bool info = false)
        {
            string? auth;
            try
            {
                auth = Request.Cookies["token"];
            }
            catch (Exception)
            {
                return Ok(new { auth = false });
            }

            if (auth == null)
                return Ok(new { auth = false });


            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://authservice/users/me?info=" + info);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth);

            var r = await Utils.client.SendAsync(requestMessage);
            var code = r.StatusCode;
            if (code == HttpStatusCode.OK)
                try
                {
                    return File(await r.Content.ReadAsStreamAsync(), r.Content.Headers.GetValues("Content-Type").First());
                }
                catch (InvalidOperationException)
                {
                    return BadRequest();
                }
            return StatusCode((int)code);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> UIndex() =>
            await MicroServiceRequest("authservice/users");

        [HttpGet("Users/Details/{id}")]
        public async Task<IActionResult> UDetails(Guid id) =>
            await MicroServiceRequest("authservice/details/" + id);

        [HttpPost("Users/Create")]
        public async Task<IActionResult> UCreate([FromBody] UserRegister user) =>
            await MicroServiceRequest("authservice/users/create", user);

        [HttpPost("Users/Edit")]
        public async Task<IActionResult> UEdit([FromBody] User user) =>
            await MSRAuth("authservice/users/edit", user);

        [HttpPost("Users/ChangePassword")]
        public async Task<IActionResult> UChangePassword([FromBody] UserRegister user) =>
            await MSRAuth("authservice/changepassword", user);

        [HttpPost("Users/Delete/{id}")]
        public async Task<IActionResult> UDelete(Guid id) =>
            await MSRAuth("authservice/delete/" + id, new { });

        [HttpGet("Brands")]
        public async Task<IActionResult> BIndex(int page = 1) =>
            await MicroServiceRequest("catalogservice/brands?page=" + page);

        [HttpGet("Brands/Details")]
        public async Task<IActionResult> BDetails(int page = 1) =>
            await MicroServiceRequest("catalogservice/brands/details?page=" + page);

        [HttpGet("Brands/{id}")]
        public async Task<IActionResult> BIndex(Guid id) =>
            await MicroServiceRequest("catalogservice/brands/" + id);

        [HttpGet("Brands/Image/{id}")]
        public async Task<IActionResult> BGetLogo(Guid id) =>
            await MicroServiceRequest("catalogservice/brands/image/" + id);

        [HttpPost("Brands/Create")]
        public async Task<IActionResult> BCreate([FromBody] Brand brand)
        {
            if (!await Authorized(Request))
                return Unauthorized();
            return await MicroServiceRequest("catalogservice/brands/create", brand);
        }

        [HttpPost("Brands/Edit")]
        public async Task<IActionResult> BEdit([FromBody] Brand brand)
        {
            if (!await Authorized(Request))
                return Unauthorized();
            return await MicroServiceRequest("catalogservice/brands/edit", brand);
        }

        [HttpPost("Brands/EditLogo/{id}")]
        public async Task<IActionResult> BEditLogo(IFormFile uploadedFile, Guid id)
        {
            if (!await Authorized(Request))
                return Unauthorized();

            return await MicroServiceRequest("catalogservice/brands/editlogo/" + id, uploadedFile, json: false);
        }

        [HttpPost("Brands/Delete/{id}")]
        public async Task<IActionResult> BDelete(Guid id)
        {
            if (!await Authorized(Request))
                return Unauthorized();
            return await MicroServiceRequest("catalogservice/brands/delete/" + id, new { });
        }

        [HttpGet("Products")]
        public async Task<IActionResult> PIndex(int page = 1, string idList = "null", string species = "null", int limit = 30) =>
            await MicroServiceRequest("catalogservice/products?page=" + page + "&idList=" + idList + "&species=" + species + "&limit=" + limit);

        [HttpGet("Products/{id}")]
        public async Task<IActionResult> PIndex(Guid id) =>
            await MicroServiceRequest("catalogservice/products/" + id);

        [HttpGet("Products/ByBrand/{id}")]
        public async Task<IActionResult> PByBrand(Guid id) =>
            await MicroServiceRequest("catalogservice/products/bybrand/" + id);

        [HttpGet("Products/Image/{id}")]
        public async Task<IActionResult> PGetPhoto(Guid id) =>
            await MicroServiceRequest("catalogservice/products/image/" + id);

        [HttpPost("Products/Create")]
        public async Task<IActionResult> PCreate([FromBody] Product product)
        {
            if (!await Authorized(Request))
                return Unauthorized();
            return await MicroServiceRequest("catalogservice/products/create", product);
        }

        [HttpPost("Products/Edit")]
        public async Task<IActionResult> PEdit([FromBody] Product product)
        {
            if (!await Authorized(Request))
                return Unauthorized();
            return await MicroServiceRequest("catalogservice/products/edit", product);
        }

        [HttpPost("Products/EditPhoto/{id}")]
        public async Task<IActionResult> PEditPhotos(IFormFile uploadedFile, Guid id)
        {
            if (!await Authorized(Request))
                return Unauthorized();

            return await MicroServiceRequest("catalogservice/products/editphoto/" + id, uploadedFile, json: false);
        }

        [HttpPost("Products/Delete/{id}")]
        public async Task<IActionResult> PDelete(Guid id)
        {
            await Authorized(Request);
            return await MicroServiceRequest("catalogservice/products/delete/" + id, new { });
        }

        [HttpGet("Cart")]
        public async Task<IActionResult> CGet()
        {
            var userId = await GetUserId(Request);
            if (userId == null)
                return Unauthorized();
            return await MicroServiceRequest("cartservice/cart?userId=" + userId);
        }

        [HttpPost("Cart/Add/{productId}")]
        public async Task<IActionResult> CAdd(string productId)
        {
            var userId = await GetUserId(Request);
            if (userId == null)
                return Unauthorized();
            return await MicroServiceRequest("cartservice/cart/add?userId=" + userId + "&productId=" + productId);
        }

        [HttpPost("Cart/Remove/{productId}")]
        public async Task<IActionResult> CRemove(string productId)
        {
            var userId = await GetUserId(Request);
            if (userId == null)
                return Unauthorized();
            return await MicroServiceRequest("cartservice/cart/remove?userId=" + userId + "&productId=" + productId);
        }

        [HttpPost("Cart/ChangeCount/{productId}")]
        public async Task<IActionResult> CCount(string productId, int count)
        {
            var userId = await GetUserId(Request);
            if (userId == null)
                return Unauthorized();
            return await MicroServiceRequest("cartservice/cart/ChangeCount?userId=" + userId + "&productId=" + productId + "&count=" + count);
        }

        [HttpPost("Cart/Clear")]
        public async Task<IActionResult> CClear()
        {
            var userId = await GetUserId(Request);
            if (userId == null)
                return Unauthorized();
            return await MicroServiceRequest("cartservice/cart/clear?userId=" + userId);
        }
    }
}