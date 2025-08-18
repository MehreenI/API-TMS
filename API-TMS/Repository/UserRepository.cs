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
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
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
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
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
                _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
                throw;
        }
        }

        public async Task<List<GetUserDto>> GetAllAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Tasks)
                            .Select(u => new GetUserDto
                            {
                                Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                                Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                                Role = u.Role,
                        CreatedAt = u.CreatedAt,
                        LastLogin = u.LastLogin,
                                TaskCount = u.Tasks.Count
                            })
                            .ToListAsync();

                return users;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
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
                _logger.LogError(ex, "Error creating user with email: {Email}", user.Email);
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
                _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
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
                _logger.LogError(ex, "Error updating password for user ID: {UserId}", userId);
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
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
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
                _logger.LogError(ex, "Error checking if email exists: {Email}", email);
                throw;
            }
        }
    }
}
