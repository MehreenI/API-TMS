using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using API_TMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                    return Unauthorized("Invalid email or password");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return Unauthorized("Invalid email or password");

                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                var token = _jwtService.GenerateToken(user);
                var response = _jwtService.CreateAuthResponse(user, token);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
                return StatusCode(500, "An error occurred during login");
            }
        }

        [HttpPost("validate-token")]
        public ActionResult ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = _jwtService.ValidateToken(token);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token validation");
                return StatusCode(500, "An error occurred during token validation");
            }
        }
    }
}
