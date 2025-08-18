using API_TMS.Models;

namespace API_TMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMailService _mailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IMailService mailService, ILogger<NotificationService> logger)
        {
            _mailService = mailService;
            _logger = logger;
        }

        public async Task SendTaskDeadlineNotificationAsync(TaskItem task)
        {
            try
            {
                if (task.AssignedUser == null) return;

                var timeRemaining = task.Deadline - DateTime.UtcNow;
                var hoursRemaining = (int)timeRemaining.TotalHours;

                var mailData = new Mail
                {
                    EmailToId = task.AssignedUser.Email,
                    EmailToName = task.AssignedUser.FullName,
                    EmailSubject = $"Task Deadline Alert: {task.Title}",
                    EmailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #d32f2f;'>⚠️ Task Deadline Alert</h2>
                            <p>Hello {task.AssignedUser.FullName},</p>
                            <p>This is a reminder that your task <strong>'{task.Title}'</strong> is due soon.</p>
                            
                            <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3>Task Details:</h3>
                                <p><strong>Title:</strong> {task.Title}</p>
                                <p><strong>Description:</strong> {task.Description}</p>
                                <p><strong>Priority:</strong> {task.Priority}</p>
                                <p><strong>Status:</strong> {task.Status}</p>
                                <p><strong>Deadline:</strong> {task.Deadline:MMM dd, yyyy 'at' HH:mm}</p>
                                <p><strong>Time Remaining:</strong> {hoursRemaining} hours</p>
                            </div>
                            
                            <p>Please ensure this task is completed before the deadline.</p>
                            <p>Best regards,<br/>TaskFlow Pro Team</p>
                        </div>"
                };

                await _mailService.SendMail(mailData);
                _logger.LogInformation("Deadline notification sent for task {TaskId} to {Email}", 
                    task.Id, task.AssignedUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending deadline notification for task {TaskId}", task.Id);
            }
        }

        public async Task SendTaskAssignmentNotificationAsync(TaskItem task)
        {
            try
            {
                if (task.AssignedUser == null) return;

                var mailData = new Mail
                {
                    EmailToId = task.AssignedUser.Email,
                    EmailToName = task.AssignedUser.FullName,
                    EmailSubject = $"New Task Assigned: {task.Title}",
                    EmailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #1976d2;'>📋 New Task Assignment</h2>
                            <p>Hello {task.AssignedUser.FullName},</p>
                            <p>A new task has been assigned to you.</p>
                            
                            <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3>Task Details:</h3>
                                <p><strong>Title:</strong> {task.Title}</p>
                                <p><strong>Description:</strong> {task.Description}</p>
                                <p><strong>Priority:</strong> {task.Priority}</p>
                                <p><strong>Deadline:</strong> {task.Deadline:MMM dd, yyyy 'at' HH:mm}</p>
                                <p><strong>Status:</strong> {task.Status}</p>
                            </div>
                            
                            <p>Please review the task details and update the status as you progress.</p>
                            <p>Best regards,<br/>TaskFlow Pro Team</p>
                        </div>"
                };

                await _mailService.SendMail(mailData);
                _logger.LogInformation("Assignment notification sent for task {TaskId} to {Email}", 
                    task.Id, task.AssignedUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assignment notification for task {TaskId}", task.Id);
            }
        }

        public async Task SendWelcomeNotificationAsync(User user, string tempPassword)
        {
            try
            {
                var mailData = new Mail
                {
                    EmailToId = user.Email,
                    EmailToName = user.FullName,
                    EmailSubject = "Welcome to TaskFlow Pro",
                    EmailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #388e3c;'>🎉 Welcome to TaskFlow Pro!</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Your account has been created successfully. Welcome to our Task Management System!</p>
                            
                            <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3>Account Details:</h3>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p><strong>Role:</strong> {user.Role}</p>
                                <p><strong>Temporary Password:</strong> {tempPassword}</p>
                            </div>
                            
                            <div style='background-color: #fff3e0; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ff9800;'>
                                <p><strong>⚠️ Important:</strong> Please change your password after your first login for security purposes.</p>
                            </div>
                            
                            <p>You can now log in to the system and start managing your tasks.</p>
                            <p>Best regards,<br/>TaskFlow Pro Team</p>
                        </div>"
                };

                await _mailService.SendMail(mailData);
                _logger.LogInformation("Welcome notification sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome notification to {Email}", user.Email);
            }
        }
    }
}
