using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using API_TMS.Services;
using Microsoft.AspNetCore.Authorization;
using API_TMS.Dtos;
using System.Linq;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public UsersController(IUserRepository userRepository, INotificationService notificationService)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<CurrentUserDto>> UpdateMyProfile([FromBody] UpdateProfileDto request)
        {
            var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;
            if (!string.IsNullOrWhiteSpace(request.ProfileImagePath)) user.ProfileImagePath = request.ProfileImagePath;

            var updated = await _userRepository.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _userRepository.UpdatePasswordAsync(user.Id, hashedPassword);

            }
            if (updated == null)
                return NotFound();

            var dto = new CurrentUserDto
            {
                Id = updated.Id,
                FirstName = updated.FirstName,
                LastName = updated.LastName,
                Email = updated.Email,
                PhoneNumber = updated.PhoneNumber,
                DateOfBirth = updated.DateOfBirth,
                ProfileImagePath = updated.ProfileImagePath,
                Role = updated.Role
            };

            return Ok(dto);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<CurrentUserDto>> GetMe()
        {
            var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            var dto = new CurrentUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                ProfileImagePath = user.ProfileImagePath,
                Role = user.Role
            };

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var result = users.Select(MapToGetUserDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(MapToGetUserDto(user));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User request)
        {
            if (request == null)
                return BadRequest("User cannot be null");

            if (await _userRepository.EmailExistsAsync(request.Email))
                return Conflict("Email already exists");

            var tempPassword = GenerateRandomPassword(10);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = hashedPassword,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
                ProfileImagePath = request.ProfileImagePath,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);

            try
            {
                await _notificationService.SendWelcomeNotificationAsync(createdUser, tempPassword);
            }
            catch (Exception ex)
            {
                // log error instead of throw
            }

            return Ok(MapToGetUserDto(createdUser));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (user == null || user.Id != id)
                return BadRequest("Invalid User data");

            var existingUser = await _userRepository.UpdateAsync(user);
            if (existingUser == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(id, hashedPassword);

            return Ok("Password changed successfully");
        }


        private static string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static GetUserDto MapToGetUserDto(User user)
        {
            return new GetUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                TaskCount = user.Tasks?.Count ?? 0
            };
        }
    }

}
