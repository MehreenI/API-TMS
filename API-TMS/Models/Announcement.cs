using System.ComponentModel.DataAnnotations;

namespace API_TMS.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public required string Subject { get; set; }

        [Required(ErrorMessage = "Message body is required.")]
        public required string Body { get; set; }

        public DateTime CreatedAt { get; set; } = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc);
    }
}
