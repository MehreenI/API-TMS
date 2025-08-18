using API_TMS.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_TMS.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> options)
        {
            _jwtSettings = options.Value;
        }

        public string CreateToken(API_TMS.Models.User user)
        {
            var jwtKey = _jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT Key is not configured");
            var jwtIssuer = _jwtSettings.Issuer ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var jwtAudience = _jwtSettings.Audience ?? throw new InvalidOperationException("JWT Audience is not configured");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(300),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var jwtKey = _jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT Key is not configured");
                var jwtIssuer = _jwtSettings.Issuer ?? throw new InvalidOperationException("JWT Issuer is not configured");
                var jwtAudience = _jwtSettings.Audience ?? throw new InvalidOperationException("JWT Audience is not configured");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}