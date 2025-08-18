using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using API_TMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskItemsController : ControllerBase
    {
        private readonly ITaskItemRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskItemsController> _logger;

        public TaskItemsController(
            ITaskItemRepository taskRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<TaskItemsController> logger)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll(
            [FromQuery] int? assignedUserId,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] DateTime? deadline)
        {
            try
            {
            var tasks = await _taskRepository.GetFilteredAsync(assignedUserId, priority, status, deadline);

            var response = tasks.Select(MapToDto);
            return Ok(response);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered tasks");
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }

        [HttpGet("my-tasks")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetMyTasks(
            int userId,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] DateTime? deadline)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var tasks = await _taskRepository.GetByAssignedUserAsync(currentUserId);

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TaskPriority>(priority, true, out var parsedPriority))
                    tasks = tasks.Where(t => t.Priority == parsedPriority).ToList();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TStatus>(status, true, out var parsedStatus))
                    tasks = tasks.Where(t => t.Status == parsedStatus).ToList();

            if (deadline.HasValue)
                    tasks = tasks.Where(t => t.Deadline.Date == deadline.Value.Date).ToList();

            var response = tasks.Select(MapToDto);
            return Ok(response);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user tasks");
                return StatusCode(500, "An error occurred while retrieving your tasks");
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<TaskResponseDto>> GetById(int id)
        {
            try
            {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound("Task not found");

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserRole != "Admin" && task.AssignedUserId != currentUserId)
                    return Forbid();

            return Ok(MapToDto(task));
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID: {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<TaskResponseDto>> Create([FromBody] TaskCreateDto request)
        {
            try
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

                if (request.AssignedUserId.HasValue)
                {
                    var assignedUser = await _userRepository.GetByIdAsync(request.AssignedUserId.Value);
                    if (assignedUser == null)
                        return BadRequest("Assigned user not found");
                }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Deadline = request.Deadline,
                    Priority = request.Priority,
                    Status = TStatus.ToDo,
                    AssignedUserId = request.AssignedUserId,
                    CreatedAt = DateTime.UtcNow
            };

            var createdTask = await _taskRepository.CreateAsync(task);
                
                try
                {
                    if (createdTask.AssignedUserId.HasValue)
                    {
                        var taskWithUser = await _taskRepository.GetByIdAsync(createdTask.Id);
                        if (taskWithUser != null)
                        {
                            await _notificationService.SendTaskAssignmentNotificationAsync(taskWithUser);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send task assignment notification for task {TaskId}", createdTask.Id);
                }
                
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, MapToDto(createdTask));
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task with title: {Title}", request.Title);
                return StatusCode(500, "An error occurred while creating the task");
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto request)
        {
            try
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return NotFound("Task not found");

                if (request.AssignedUserId.HasValue)
                {
                    var assignedUser = await _userRepository.GetByIdAsync(request.AssignedUserId.Value);
                    if (assignedUser == null)
                        return BadRequest("Assigned user not found");
                }

            existingTask.Title = request.Title ?? existingTask.Title;
            existingTask.Description = request.Description ?? existingTask.Description;
            existingTask.Deadline = request.Deadline ?? existingTask.Deadline;
            existingTask.Priority = request.Priority ?? existingTask.Priority;
            existingTask.Status = request.Status ?? existingTask.Status;
            existingTask.AssignedUserId = request.AssignedUserId ?? existingTask.AssignedUserId;
                existingTask.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(existingTask);
            return NoContent();
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID: {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task");
            }
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskUpdateStatusDto request)
        {
            try
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return NotFound("Task not found");

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserRole != "Admin" && existingTask.AssignedUserId != currentUserId)
                    return Forbid();

                existingTask.Status = request.Status;
                existingTask.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(existingTask);
            return NoContent();
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for task ID: {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task status");
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
            var result = await _taskRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Task not found");

            return NoContent();
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID: {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }

        [HttpGet("due-soon")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetDueSoon([FromQuery] int hours = 24)
        {
            try
            {
            var tasks = await _taskRepository.GetDueSoonAsync(hours);
            var response = tasks.Select(MapToDto).ToList();

            // force IsDueSoon = true since repo already filters
            response.ForEach(r => r.IsDueSoon = true);

            return Ok(response);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks due soon within {Hours} hours", hours);
                return StatusCode(500, "An error occurred while retrieving due soon tasks");
            }
        }

        [HttpGet("dashboard-stats")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                List<TaskItem> tasks;
                if (currentUserRole == "Admin")
                {
                    tasks = await _taskRepository.GetAllAsync();
                }
                else
                {
                    tasks = await _taskRepository.GetByAssignedUserAsync(currentUserId);
                }

                var stats = new DashboardStatsDto
                {
                    TotalTasks = tasks.Count,
                    CompletedTasks = tasks.Count(t => t.Status == TStatus.Done),
                    InProgressTasks = tasks.Count(t => t.Status == TStatus.InProgress),
                    PendingTasks = tasks.Count(t => t.Status == TStatus.ToDo),
                    DueSoonTasks = tasks.Count(t => t.Deadline <= DateTime.UtcNow.AddHours(24) && t.Status != TStatus.Done),
                    HighPriorityTasks = tasks.Count(t => t.Priority == TaskPriority.High && t.Status != TStatus.Done)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, "An error occurred while retrieving dashboard statistics");
            }
        }

        private TaskResponseDto MapToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.FullName,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsDueSoon = task.Deadline <= DateTime.UtcNow.AddHours(24) && task.Status != TStatus.Done
            };
    }
}
}
