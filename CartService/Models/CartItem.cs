using Redis.OM.Modeling;

namespace CartService.Models
{
    public class CartItem
    {
        [Indexed] public string? ProductId { get; set; }
        [Indexed] public int Count { get; set; } = 1;
    }
}