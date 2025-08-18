namespace API_TMS.Dtos
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResponse
    {
        public required string Token { get; set; }
        public required int UserId { get; set; }
        public required string Role { get; set; }
        public required string FullName { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmNewPassword { get; set; }
    }
}
