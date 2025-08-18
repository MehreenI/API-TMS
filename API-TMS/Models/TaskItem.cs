using System.ComponentModel.DataAnnotations;

namespace API_TMS.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Deadline is required.")]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public TStatus Status { get; set; }

        public int? AssignedUserId { get; set; }
        public virtual User? AssignedUser { get; set; }

        public string? Attachments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public enum TStatus
    {
        ToDo,
        InProgress,
        Done
    }
}
