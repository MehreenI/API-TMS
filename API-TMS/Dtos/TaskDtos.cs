namespace API_TMS.Dtos
{
    // ✅ DTO for creating a task
    public class TaskCreateDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime Deadline { get; set; }
        public string? Priority { get; set; } = "Medium"; // default priority
        public string? Status { get; set; } = "Pending";  // default status
        public required int AssignedUserId { get; set; }
    }

    // ✅ DTO for updating a task (partial updates allowed)
    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? AssignedUserId { get; set; }
    }

    // ✅ DTO for updating only the status of a task
    public class TaskUpdateStatusDto
    {
        public required string Status { get; set; }
    }

    // ✅ DTO for returning a task response
    public class TaskResponseDto
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime Deadline { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Pending";
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public bool IsDueSoon { get; set; }
    }

    // ✅ DTO for task analytics
    public class TaskAnalysisDto
    {
        public string UserEmail { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
    }
}
