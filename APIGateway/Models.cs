using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace APIGateway
{
    public class Models
    {
        public class User
        {
            public Guid? Id { get; set; }
            public string? Login { get; set; }
            public string? PasswordHash { get; set; }
            public string? Firstname { get; set; }
            public string? Lastname { get; set; }
            public string? Middlename { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public bool IsAdmin { get; set; }
        }

        public class UserRegister
        {
            public string? Login { get; set; }
            public string? Password { get; set; }
        }

        public class Product
        {
            public Guid? Id { get; set; }
            public string? Model { get; set; }
            public Brand? Brand { get; set; }
            public Guid? BrandId { get; set; }
            public byte[]? Photo1 { get; set; }
            public byte[]? Photo2 { get; set; }
            public byte[]? Photo3 { get; set; }
            public string? Resolution { get; set; }
            public string? Color { get; set; }
            public JsonDocument? OtherSpecs { get; set; }
        }

        public class Brand
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? Country { get; set; }
            public string? Website { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public byte[]? Logo { get; set; }
            public string? Description { get; set; }
            public int? Year { get; set; }
        }
    }
}
