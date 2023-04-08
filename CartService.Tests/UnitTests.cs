using CartService.Controllers;
using CartService.Models;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using Xunit.Abstractions;

namespace CartService.Tests
{
    public class UnitTests
    {
        private CartController controller;

        public UnitTests(ITestOutputHelper output)
        {
            var conn = new RedisConnectionProvider("redis://localhost:6380");
            conn.Connection.CreateIndexAsync(typeof(User));
            controller = new CartController(conn);
        }

        [Fact]
        public async Task Cart()
        {
            controller.Clear("user");
            await controller.AddProduct("user", "product1");
            await controller.AddProduct("user", "product2");
            await controller.AddProduct("user", "product3");
            await controller.RemoveProduct("user", "product2");

            var ret = await controller.GetCart("user");
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);
            var list = ok.Value as CartItem[];
            Assert.Equal(2, list.Length);
            Assert.Equal("product1", list[0].ProductId);
            Assert.Equal("product3", list[1].ProductId);

            controller.Clear("user");

            var ret2 = await controller.GetCart("user");
            Assert.NotNull(ret2);
            var ok2 = Assert.IsType<OkObjectResult>(ret2);
            var list2 = ok2.Value as CartItem[];
            Assert.Equal(0, list2.Length);
        }
    }
}