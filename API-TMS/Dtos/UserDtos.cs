using System;
using System.ComponentModel.DataAnnotations;

namespace API_TMS.Dtos
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        public string? Role { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? Password { get; set; }
    }

    public class ProfileDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImagePath { get; set; }
        public int? TaskCount { get; set; }
    }

    public class GetUserDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int TaskCount { get; set; }
    }

    public class CurrentUserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImagePath { get; set; }
    }

}
