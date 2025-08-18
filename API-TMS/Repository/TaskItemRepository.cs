using API_TMS.Data;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Repository
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskItemRepository> _logger;

        public TaskItemRepository(AppDbContext context, ILogger<TaskItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID: {TaskId}", id);
                throw;
            }
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            try
            {
                return await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tasks");
                throw;
            }
        }

        public async Task<List<TaskItem>> GetAllWithUsersAsync()
        {
            try
            {
                return await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks with users");
                throw;
            }
        }

        public async Task<List<TaskItem>> GetFilteredAsync(int? assignedUserId, string? priority, string? status, DateTime? deadline)
        {
            try
            {
                var query = _context.TaskItems.Include(t => t.AssignedUser).AsQueryable();

                if (assignedUserId.HasValue)
                    query = query.Where(t => t.AssignedUserId == assignedUserId.Value);

                if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TaskPriority>(priority, true, out var parsedPriority))
                    query = query.Where(t => t.Priority == parsedPriority);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<TStatus>(status, true, out var parsedStatus))
                    query = query.Where(t => t.Status == parsedStatus);

                if (deadline.HasValue)
                    query = query.Where(t => t.Deadline.Date == deadline.Value.Date);

                return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering tasks");
                throw;
            }
        }

        public async Task<List<TaskItem>> GetByAssignedUserAsync(int userId)
        {
            try
            {
                return await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .Where(t => t.AssignedUserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<TaskItem>> GetDueSoonAsync(int hours = 24)
        {
            try
            {
                var deadlineThreshold = DateTime.UtcNow.AddHours(hours);
                return await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .Where(t => t.Deadline <= deadlineThreshold && t.Status != TStatus.Done)
                    .OrderBy(t => t.Deadline)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks due soon within {Hours} hours", hours);
                throw;
            }
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            try
            {
                task.CreatedAt = DateTime.UtcNow;
                _context.TaskItems.Add(task);
                await _context.SaveChangesAsync();
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task with title: {Title}", task.Title);
                throw;
            }
        }

        public async Task<TaskItem?> UpdateAsync(TaskItem task)
        {
            try
            {
                var existingTask = await _context.TaskItems.FindAsync(task.Id);
                if (existingTask == null)
                    return null;

                task.UpdatedAt = DateTime.UtcNow;
                _context.Entry(existingTask).CurrentValues.SetValues(task);
                await _context.SaveChangesAsync();
                return existingTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID: {TaskId}", task.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var task = await _context.TaskItems.FindAsync(id);
                if (task == null)
                    return false;

                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID: {TaskId}", id);
                throw;
            }
        }
    }
}
