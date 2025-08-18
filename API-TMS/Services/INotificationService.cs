using API_TMS.Models;

namespace API_TMS.Services
{
    public interface INotificationService
    {
        Task SendTaskDeadlineNotificationAsync(TaskItem task, string notificationType = "default");
        Task SendTaskAssignmentNotificationAsync(TaskItem task);
        Task SendWelcomeNotificationAsync(User user, string tempPassword);
    }
}
