using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService
{
    public class AuthOptions
    {
        public const string ISSUER = "ProjShopAuthMS";
        public const string AUDIENCE = "ProjShopWeb";
        public static readonly TimeSpan LIFETIME = TimeSpan.FromMinutes(60 * 24 * 7);
        public static readonly SymmetricSecurityKey KEY = new(Encoding.ASCII.GetBytes("mysupersecret_secretkey!123"));
        public static string GetToken(Guid? id, DateTime now) =>
            new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: ISSUER,
                audience: AUDIENCE,
                expires: now.Add(LIFETIME),
                notBefore: now,
                claims: new List<Claim> { new(ClaimsIdentity.DefaultNameClaimType, id?.ToString() ?? throw new ArgumentNullException()) },
                signingCredentials: new(KEY, SecurityAlgorithms.HmacSha512)));
    }
}
