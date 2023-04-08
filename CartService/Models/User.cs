using Redis.OM.Modeling;

namespace CartService.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "User" })]
    public class User
    {
        [RedisIdField][Indexed] public string UserId { get; set; }
        [Indexed(CascadeDepth = 1)] public CartItem[] Items { get; set; } = Array.Empty<CartItem>();
    }
}