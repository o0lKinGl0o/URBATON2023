using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication3
{
    public class TokenHelper
    {
        private readonly ApplicationDbContext _context;

        public TokenHelper(ApplicationDbContext context)
        {
            _context = context;
        }
        public string GetCurrentTokenId(string currentToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDecoded = tokenHandler.ReadJwtToken(currentToken);
            return tokenDecoded.Claims.FirstOrDefault(claim => claim.Type == "jti")?.Value!;
        }
        public bool IsInvalidToken(string currentTokenId)
        {
            var blacklistedToken = _context.InvalidTokens.FirstOrDefault(t => t.TokenId == currentTokenId);
            return blacklistedToken != null;

        }
        public void ToInvalidToken(string currentTokenId, DateTime currentTokenExpiryDate)
        {
            var invalidToken = new InvalidToken
            {
                TokenId = currentTokenId, // Идентификатор текущего JWT
                ExpiryDate = currentTokenExpiryDate // Время истечения текущего JWT
            };
            _context.InvalidTokens.Add(invalidToken);
            _context.SaveChanges();
        }
        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Const.SecretKeyValue);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("user", username),
                    new Claim("jti", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public string ExtractUsernameFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Const.SecretKeyValue);

            try
            {
                var tokenDecoded = tokenHandler.ReadJwtToken(token);
                var expiration = tokenDecoded.ValidTo;

                if (expiration <= DateTime.UtcNow)
                {
                    return null!;
                }

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out _);

                var nameClaim = tokenDecoded.Claims.FirstOrDefault(claim => claim.Type == "user");
                return nameClaim!.Value;
            }
            catch
            {
                return null!;
            }
        }
        public DateTime GetTokenExpiryDate(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            var expirationDate = jwtToken.ValidTo;
            return expirationDate;
        }
        public bool IsTokenExpired(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token))
            {
                return true; // Если токен не может быть прочитан, считаем его недействительным
            }
            return DateTime.UtcNow > GetTokenExpiryDate(token);
        }
    }
}
