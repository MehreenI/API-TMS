using API_TMS.Models;

namespace API_TMS.Dtos
{
    // ✅ DTO for creating a task
    public class TaskCreateDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime Deadline { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public int? AssignedUserId { get; set; }
    }

    // ✅ DTO for updating a task (partial updates allowed)
    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public TaskPriority? Priority { get; set; }
        public TStatus? Status { get; set; }
        public int? AssignedUserId { get; set; }
    }

    // ✅ DTO for updating only the status of a task
    public class TaskUpdateStatusDto
    {
        public required TStatus Status { get; set; }
    }

    // ✅ DTO for returning a task response
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime Deadline { get; set; }
        public required string Priority { get; set; }
        public required string Status { get; set; }
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDueSoon { get; set; }
    }

    public class DashboardStatsDto
    {
        public string UserEmail { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
        public int DueSoonTasks { get; set; }
        public int HighPriorityTasks { get; set; }
    }
}
