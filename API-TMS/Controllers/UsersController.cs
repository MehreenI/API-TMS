using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using API_TMS.Services;
using Org.BouncyCastle.Asn1.Pkcs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<GetUserDto>>> GetAll()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<GetUserDto>> GetById(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserRole != "Admin" && currentUserId != id)
                    return Forbid();

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                var userDto = new GetUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    TaskCount = user.Tasks.Count
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<GetUserDto>> Create([FromBody] CreateUserDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _userRepository.EmailExistsAsync(request.Email))
                    return BadRequest("Email already exists");

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
                    _logger.LogWarning(ex, "Failed to send welcome notification to {Email}", createdUser.Email);
                }

                var responseDto = new GetUserDto
                {
                    Id = createdUser.Id,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Email = createdUser.Email,
                    PhoneNumber = createdUser.PhoneNumber,
                    Role = createdUser.Role,
                    CreatedAt = createdUser.CreatedAt,
                    LastLogin = createdUser.LastLogin,
                    TaskCount = 0
                };

                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserRole != "Admin" && currentUserId != id)
                    return Forbid();

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    if (await _userRepository.EmailExistsAsync(request.Email))
                        return BadRequest("Email already exists");
                }

                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.Email = request.Email ?? user.Email;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                
                if (currentUserRole == "Admin" && !string.IsNullOrEmpty(request.Role))
                    user.Role = request.Role;

                await _userRepository.UpdateAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                await _userRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        [HttpPost("{id:int}/change-password")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserRole != "Admin" && currentUserId != id)
                    return Forbid();

                if (request.NewPassword != request.ConfirmNewPassword)
                    return BadRequest("Passwords do not match");

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                    return BadRequest("Current password is incorrect");

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _userRepository.UpdatePasswordAsync(id, hashedPassword);

                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", id);
                return StatusCode(500, "An error occurred while changing the password");
            }
        }

        private static string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
