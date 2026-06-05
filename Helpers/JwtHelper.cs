using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JCAM_CONNECT.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(int userId, string email, string role)
        {
            var secretKeyString = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKeyString))
            {
                secretKeyString = "YourSuperSecretKeyHereMakeItAtLeast32CharactersLong12345";
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiryMinutesString = _configuration["JwtSettings:ExpiryMinutes"];
            if (!double.TryParse(expiryMinutesString, out double expiryMinutes))
            {
                expiryMinutes = 1440;
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "JCAMConnectAPI",
                audience: _configuration["JwtSettings:Audience"] ?? "JCAMConnectClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}