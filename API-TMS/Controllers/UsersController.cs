using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using API_TMS.Services;
using Org.BouncyCastle.Asn1.Pkcs;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMailService _emailService;

        public UsersController(IUserRepository userRepository, IMailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetUserDto>>> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] CreateUserDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tempPassword = GenerateRandomPassword(10);
            var hashed = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = hashed,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
                ProfileImagePath = request.ProfileImagePath
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // prepare welcome email
            try
            {
                var mailData = new MailData
                {
                    EmailToId = createdUser.Email,
                    EmailToName = createdUser.FullName,
                    EmailSubject = "Welcome to TaskFlow Pro",
                    EmailBody = $"Hello {createdUser.FullName},<br/>" +
                                $"Your account has been created.<br/>" +
                                $"Email: {createdUser.Email}<br/>" +
                                $"Password: {tempPassword}<br/>"
                };
                _emailService.SendMail(mailData);
            }
            catch { /* log but don’t fail */ }

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UpdateUserDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("User not found");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userRepository.DeleteAsync(id); // This line causes CS4008 because DeleteAsync is void.
            return NoContent();
        }

        [HttpPost("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (request.NewPassword != request.ConfirmNewPassword)
                return BadRequest("Passwords do not match.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                return BadRequest("Current password is incorrect.");

            var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(id, hashed);

            return Ok("Password changed successfully.");
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToCsv()
        {
            var users = await _userRepository.GetAllAsync();
            var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(users);
            writer.Flush();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "users.csv");
        }

        private static string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*()";
            var data = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var result = new char[length];
            for (int i = 0; i < length; i++) result[i] = chars[data[i] % chars.Length];
            return new string(result);
        }
        public async Task DeleteAsync(int id)
        {
            // Implementation of the delete logic
            // For example:
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<List<GetUserDto>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<User> UpdatePasswordAsync(int userId, string newPassword);
        Task DeleteAsync(int id); // Changed from void to Task
    }
}
