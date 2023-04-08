using AuthService.Controllers;
using AuthService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Dynamic;
using System.Security.Principal;
using Xunit.Abstractions;

namespace AuthService.Tests
{
    [Collection("Sequential")]
    public class UnitTests
    {
        Database database;
        UsersController controller;
        List<User> users;
        private readonly ITestOutputHelper output;

        public UnitTests(ITestOutputHelper output)
        {
            this.output = output;
            var options =
                new DbContextOptionsBuilder<Database>()
                    .UseNpgsql("Server=localhost:5532;Username=test;Password=test;Database=test")
                    .Options;

            database = new Database(options, delete: true);

            controller = new UsersController(database);

            users = new List<User>
            {
                new (){ Login = "admin", Phone = "+79994594682", PasswordHash = Utils.SHA512("admin"), IsAdmin = true },
                new (){ Login = "normal", Phone = "+79994594680", PasswordHash = Utils.SHA512("normal") },
            };

            database.Users.AddRange(users);
            database.SaveChanges();
            users = database.Users.ToList();
        }

        [Fact]
        public async Task GetToken()
        {
            var ret = await controller.GetToken(new UserRegister
            {
                Login = "admin",
                Password = "admin"
            });
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);
            Assert.IsType<string>(ok.Value);
        }

        [Fact]
        public async Task GetId()
        {
            var u = users[0];
            Assert.NotNull(u);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.GetId();
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as string;

            Assert.Equal(u.Id.ToString(), dyn);
        }

        [Fact]
        public async Task Me()
        {
            var u = users[0];
            Assert.NotNull(u);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.Me();
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var obj = ToDynamic(dyn);

            Assert.NotNull(obj);
            Assert.Equal(u.Login, obj.Login);
            Assert.Equal(u.IsAdmin, obj.IsAdmin);
            Assert.Equal(u.Id, obj.Id);
            try
            {
                Assert.True(obj.Phone != obj.Phone);
            }
            catch (RuntimeBinderException)
            {
            }
            Assert.Equal(true, obj.Auth);
        }

        [Fact]
        public async Task Me_Info()
        {
            var u = users[0];
            Assert.NotNull(u);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.Me(info: true);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var obj = ToDynamic(dyn);

            Assert.NotNull(obj);
            Assert.Equal(u.Login, obj.Login);
            Assert.Equal(u.IsAdmin, obj.IsAdmin);
            Assert.Equal(u.Id, obj.Id);
            Assert.Equal(u.Phone, obj.Phone);
        }

        [Fact]
        public async Task Me_Fail()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity("N/E"), null)
                }
            };

            var ret = await controller.Me(info: true);
            Assert.NotNull(ret);
            Assert.IsType<BadRequestResult>(ret);
        }

        [Fact]
        public async Task HaveAdminAccess_Ok()
        {
            var u = users.Where(x => x.IsAdmin).FirstOrDefault();
            Assert.NotNull(u);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.HaveAdminAccess();
            Assert.True(ret);
        }

        [Fact]
        public async Task HaveAdminAccess_Fail()
        {
            var u = users.Where(x => !x.IsAdmin).FirstOrDefault();
            Assert.NotNull(u);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.HaveAdminAccess();
            Assert.False(ret);
        }

        [Fact]
        public async Task Details()
        {
            var u = users.First();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(u.Id.ToString()), null)
                }
            };

            var ret = await controller.Details(id: (Guid)u.Id);
            Assert.NotNull(ret);
            var ok = Assert.IsType<OkObjectResult>(ret);

            var dyn = ok.Value as dynamic;
            var obj = ToDynamic(dyn);

            Assert.NotNull(obj);
            Assert.Equal(u.Login, obj.Login);
            Assert.Equal(u.Phone, obj.Phone);
        }

        private static string SHA512(string input)
        {
            try
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                using var hash = System.Security.Cryptography.SHA512.Create();
                var hashedInputBytes = hash.ComputeHash(bytes);

                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        [Fact]
        public async Task Create()
        {
            var ret = await controller.Create(new UserRegister()
            {
                Login = "NEWLOGIN",
                Password = "NEWPASS"
            });

            Assert.IsType<OkResult>(ret);

            var created = database.Users.Where(x => x.Login == "NEWLOGIN").FirstOrDefault();

            Assert.NotNull(created);
            Assert.Equal(SHA512("NEWPASS"), created.PasswordHash);
        }

        [Fact]
        public async Task Edit()
        {
            var oldU = users.First();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new GenericPrincipal(new GenericIdentity(oldU.Id.ToString()), null)
                }
            };
            var u = new User()
            {
                Id = oldU.Id,
                Email = "TEST1",
                Address = "TEST2",
                Lastname = "TEST3",
                Middlename = "TEST4",
                Firstname = "TEST5",
                Phone = "TEST6",
                IsAdmin = !oldU.IsAdmin
            };

            var ret = await controller.Edit(u);

            Assert.IsType<OkResult>(ret);

            var changed = database.Users.Where(x => x.Id == oldU.Id).FirstOrDefault();

            Assert.NotNull(changed);
            Assert.Equal("TEST1", changed.Email);
            Assert.Equal("TEST2", changed.Address);
            Assert.Equal("TEST3", changed.Lastname);
            Assert.Equal("TEST4", changed.Middlename);
            Assert.Equal("TEST5", changed.Firstname);
            Assert.Equal("TEST6", changed.Phone);
            Assert.Equal(oldU.IsAdmin, changed.IsAdmin);
        }

        public List<dynamic> ToDynamicList(dynamic value)
        {
            var list = new List<dynamic>();

            foreach (var item in value)
                list.Add(ToDynamic(item));

            return list;
        }

        public dynamic ToDynamic(object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                output.WriteLine("{0}={1}", property.Name, property.GetValue(value) ?? "");
                expando.Add(property.Name, property.GetValue(value) ?? "");
            }

            return expando as ExpandoObject;
        }
    }
}