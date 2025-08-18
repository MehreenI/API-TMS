using API_TMS.Data;
using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async void DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GetUserDto>> GetAllAsync()
        {
            return await _context.Users
                            .AsNoTracking()
                            .Select(u => new GetUserDto
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                Email = u.Email,
                                Role = u.Role,
                                TaskCount = u.Tasks.Count
                            })
                            .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<User?> UpdateAsync(User user)
        {
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            user.Password = newPassword;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }


    }
}
