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
        private readonly IJwtService _tokenService;

        public AuthController(IUserRepository userRepository, IJwtService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Invalid email or password");

            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAtUtc = DateTime.Now.AddMinutes(30)
            });
        }

        [HttpPost("validate-token")]
        public ActionResult ValidateToken([FromBody] string token)
        {
            var isValid = _tokenService.ValidateToken(token);
            return Ok(new { IsValid = isValid });
        }
    }
}