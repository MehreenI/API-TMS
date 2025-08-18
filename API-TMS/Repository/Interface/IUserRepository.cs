using API_TMS.Dtos;
using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<List<GetUserDto>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<User> UpdatePasswordAsync(int userId, string newPassword);
        void DeleteAsync(int id);

    }
}
