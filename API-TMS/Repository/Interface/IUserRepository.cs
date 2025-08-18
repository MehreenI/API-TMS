using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<User> UpdatePasswordAsync(int userId, string newPassword);
        Task DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}
