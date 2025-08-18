using API_TMS.Dtos;
using API_TMS.Models;

namespace API_TMS.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        AuthResponse CreateAuthResponse(User user, string token);
    }
}
