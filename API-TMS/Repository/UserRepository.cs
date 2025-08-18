using API_TMS.Data;
using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API_TMS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Tasks)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Tasks)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Tasks)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User?> UpdateAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                    return null;

                _context.Entry(existingUser).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
                return existingUser;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User> UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                user.Password = newPassword;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
