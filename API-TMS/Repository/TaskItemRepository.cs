using API_TMS.Data;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Repository
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly AppDbContext _context;

        public TaskItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
                throw new InvalidOperationException("Task not found");

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Deadline = task.Deadline;
            existingTask.Priority = task.Priority;
            existingTask.Status = task.Status;
            existingTask.AssignedUserId = task.AssignedUserId;
            existingTask.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetFilteredAsync(
            int? assignedUserId = null,
            string? priority = null,
            string? status = null,
            DateTime? deadline = null)
        {
            var query = _context.Tasks.Include(t => t.AssignedUser).AsQueryable();

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

        public async Task<IEnumerable<TaskItem>> GetDueSoonAsync(int hoursThreshold = 24)
        {
            var threshold = DateTime.UtcNow.AddHours(hoursThreshold);
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.Deadline <= threshold && t.Status != TStatus.Done)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetAllWithUsersAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
