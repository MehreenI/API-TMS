using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface ITaskItemRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId);
        Task<TaskItem?> GetByIdAsync(int id);
        Task<List<TaskItem>> GetAllAsync();
        Task<List<TaskItem>> GetAllWithUsersAsync();
        Task<List<TaskItem>> GetFilteredAsync(int? assignedUserId, string? priority, string? status, DateTime? deadline);
        Task<List<TaskItem>> GetByAssignedUserAsync(int userId);
        Task<List<TaskItem>> GetDueSoonAsync(int hours = 24);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem?> UpdateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TaskItem>> GetFilteredAsync(
            int? assignedUserId = null,
            string? priority = null,
            string? status = null,
            DateTime? deadline = null);
        Task<IEnumerable<TaskItem>> GetDueSoonAsync(int hoursThreshold = 24);
        Task<IEnumerable<TaskItem>> GetAllWithUsersAsync();
    }
}
