using System.ComponentModel.DataAnnotations;

namespace API_TMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [StringLength(255)]
        public  string? Password { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public required string Role { get; set; }

        public string? ProfileImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
