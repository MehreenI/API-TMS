using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API_TMS.Configuration;
using API_TMS.Repository.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API_TMS.Configuration
{
    public class JwtAuthenticationHandler
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;

        public JwtAuthenticationHandler(
            RequestDelegate next,
            IOptions<JwtSettings> jwtSettings,
            IUserRepository userRepository)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
            _userRepository = userRepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    var principal = ValidateToken(token);
                    if (principal != null)
                    {
                        context.User = principal;
                    }
                }

                await _next(context);
            }
            catch
            {
                await _next(context);
            }
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
