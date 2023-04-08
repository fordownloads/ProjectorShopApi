using CartService.Models;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using Redis.OM.Searching;

namespace CartService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly RedisConnectionProvider _redis;
        private readonly RedisCollection<User> _cartUsers;

        public CartController(RedisConnectionProvider provider)
        {
            _redis = provider;
            _cartUsers = (RedisCollection<User>)provider.RedisCollection<User>();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var items = (await _cartUsers.FindByIdAsync(userId))?.Items;

            if (items == null)
                return Ok(Array.Empty<CartItem>());

            return Ok(items);
        }

        [HttpGet("Add")]
        public async Task<IActionResult> AddProduct(string userId, string productId)
        {
            var items = await _cartUsers.FindByIdAsync(userId);

            if (items != null) {
                var newI = new CartItem[items.Items.Length + 1];
                Array.Copy(items.Items, newI, items.Items.Length);
                newI[items.Items.Length] = new() { ProductId = productId, Count = 1 };
                _redis.Connection.Unlink($"User:{userId}");

                await _cartUsers.InsertAsync(new()
                {
                    UserId = userId,
                    Items = newI
                });
            }
            else
                await _cartUsers.InsertAsync(new()
                {
                    UserId = userId,
                    Items = new[] { new CartItem() { ProductId = productId, Count = 1 } }
                });

            await _cartUsers.SaveAsync();
            return Ok("true");
        }

        [HttpGet("Remove")]
        public async Task<IActionResult> RemoveProduct(string userId, string productId)
        {
            var items = (await _cartUsers.FindByIdAsync(userId))?.Items;

            if (items == null)
                return NotFound();

            _redis.Connection.Unlink($"User:{userId}");
            await _cartUsers.InsertAsync(new()
            {
                UserId = userId,
                Items = items.Where(x => x.ProductId != productId).ToArray()
            });

            await _cartUsers.SaveAsync();
            return Ok("true");
        }

        [HttpGet("Clear")]
        public IActionResult Clear(string userId)
        {
            _redis.Connection.Unlink($"User:{userId}");
            return Ok("true");
        }
    }
}