using API_TMS.Dtos;
using API_TMS.Models;

namespace API_TMS.Services
{
    public interface IJwtService
    {
        string CreateToken(User user);
        bool ValidateToken(string token);
    }
}
