using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface ITaskItemRepository
    {
        Task<TaskItem?> GetByIdAsync(int id);
        Task<List<TaskItem>> GetAllAsync();
        Task<List<TaskItem>> GetAllWithUsersAsync();
        Task<List<TaskItem>> GetFilteredAsync(int? assignedUserId, string? priority, string? status, DateTime? deadline);
        Task<List<TaskItem>> GetByAssignedUserAsync(int userId);
        Task<List<TaskItem>> GetDueSoonAsync(int hours = 24);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem?> UpdateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
    }
}
