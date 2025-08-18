using System.ComponentModel.DataAnnotations;

namespace API_TMS.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        public required string TemaplteName { get; set; }
        public required string FromAddress { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(150, ErrorMessage = "Subject cannot exceed 150 characters.")]
        public required string Subject { get; set; }

        [Required(ErrorMessage = "Body is required.")]
        public required string Body { get; set; }
    }
}
