using API_TMS.Dtos;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemsController : ControllerBase
    {
        private readonly ITaskItemRepository _taskRepository;

        public TaskItemsController(ITaskItemRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll(
            [FromQuery] int? assignedUserId,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] DateTime? deadline)
        {
            var tasks = await _taskRepository.GetFilteredAsync(assignedUserId, priority, status, deadline);

            var response = tasks.Select(MapToDto);
            return Ok(response);
        }

        [HttpGet("my/{userId:int}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetMyTasks(
            int userId,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] DateTime? deadline)
        {
            var tasks = await _taskRepository.GetByAssignedUserAsync(userId);

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TaskPriority>(priority, true, out var parsedPriority))
                tasks = tasks.Where(t => t.Priority == parsedPriority);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TStatus>(status, true, out var parsedStatus))
                tasks = tasks.Where(t => t.Status == parsedStatus);

            if (deadline.HasValue)
                tasks = tasks.Where(t => t.Deadline.Date == deadline.Value.Date);

            var response = tasks.Select(MapToDto);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskResponseDto>> GetById(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound("Task not found");

            return Ok(MapToDto(task));
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> Create([FromBody] TaskCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Deadline = request.Deadline,
                Priority = request.Priority ?? "Low",
                Status = request.Status,
                AssignedUserId = request.AssignedUserId
            };

            var createdTask = await _taskRepository.CreateAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, MapToDto(createdTask));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return NotFound("Task not found");

            existingTask.Title = request.Title ?? existingTask.Title;
            existingTask.Description = request.Description ?? existingTask.Description;
            existingTask.Deadline = request.Deadline ?? existingTask.Deadline;
            existingTask.Priority = request.Priority ?? existingTask.Priority;
            existingTask.Status = request.Status ?? existingTask.Status;
            existingTask.AssignedUserId = request.AssignedUserId ?? existingTask.AssignedUserId;

            await _taskRepository.UpdateAsync(existingTask);
            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskUpdateStatusDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return NotFound("Task not found");

            existingTask.Status = request.Status ?? 'To Do';
            await _taskRepository.UpdateAsync(existingTask);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Task not found");

            return NoContent();
        }

        [HttpGet("due-soon")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetDueSoon([FromQuery] int hours = 24)
        {
            var tasks = await _taskRepository.GetDueSoonAsync(hours);
            var response = tasks.Select(MapToDto).ToList();

            // force IsDueSoon = true since repo already filters
            response.ForEach(r => r.IsDueSoon = true);

            return Ok(response);
        }

        [HttpGet("taskcsv")]
        public async Task<IActionResult> ExportTaskAnalysisCsv()
        {
            var tasks = await _taskRepository.GetAllWithUsersAsync();

            var analysisData = tasks
                .GroupBy(t => t.AssignedUser?.Email ?? "Unknown")
                .Select(g => new TaskAnalysisDto
                {
                    UserEmail = g.Key,
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(t => t.Status == TStatus.Done),
                    InProgressTasks = g.Count(t => t.Status == TStatus.InProgress),
                    PendingTasks = g.Count(t => t.Status == TStatus.ToDo)
                })
                .ToList();

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(analysisData);
            writer.Flush();

            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "task_analysis.csv");
        }

        private TaskResponseDto MapToDto(TaskItem t) =>
            new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                AssignedUserId = t.AssignedUserId,
                AssignedUserName = t.AssignedUser?.FullName,
                CreatedAt = t.CreatedAt,
                IsDueSoon = t.Deadline <= DateTime.UtcNow.AddHours(24) && t.Status != TStatus.Done
            };
    }
}
