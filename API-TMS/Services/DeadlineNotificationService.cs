using API_TMS.Repository.Interface;
using API_TMS.Configuration;
using Microsoft.Extensions.Options;

namespace API_TMS.Services
{
    public class DeadlineNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeadlineNotificationService> _logger;
        private readonly NotificationSettings _settings;

        public DeadlineNotificationService(
            IServiceProvider serviceProvider,
            ILogger<DeadlineNotificationService> logger,
            IOptions<NotificationSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Deadline Notification Service started. Checking every {Hours} hour(s)", _settings.CheckIntervalHours);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckDeadlinesAndNotifyAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking deadlines");
                }

                var checkInterval = TimeSpan.FromHours(_settings.CheckIntervalHours);
                await Task.Delay(checkInterval, stoppingToken);
            }
        }

        private async Task CheckDeadlinesAndNotifyAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskItemRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Check tasks due in different time ranges
                var earlyWarningTasks = await taskRepository.GetDueSoonAsync(_settings.EarlyWarningHours);
                var urgentWarningTasks = await taskRepository.GetDueSoonAsync(_settings.UrgentWarningHours);
                var criticalWarningTasks = await taskRepository.GetDueSoonAsync(_settings.CriticalWarningHours);

                var totalNotifications = 0;

                // Send early warning notifications (24 hours)
                foreach (var task in earlyWarningTasks.Where(t => t.Status != Models.TStatus.Done))
                {
                    if (await ShouldSendNotificationAsync(task, _settings.EarlyWarningHours))
                    {
                        await notificationService.SendTaskDeadlineNotificationAsync(task, "early");
                        totalNotifications++;
                    }
                }

                // Send urgent warning notifications (6 hours)
                foreach (var task in urgentWarningTasks.Where(t => t.Status != Models.TStatus.Done))
                {
                    if (await ShouldSendNotificationAsync(task, _settings.UrgentWarningHours))
                    {
                        await notificationService.SendTaskDeadlineNotificationAsync(task, "urgent");
                        totalNotifications++;
                    }
                }

                // Send critical warning notifications (1 hour)
                foreach (var task in criticalWarningTasks.Where(t => t.Status != Models.TStatus.Done))
                {
                    if (await ShouldSendNotificationAsync(task, _settings.CriticalWarningHours))
                    {
                        await notificationService.SendTaskDeadlineNotificationAsync(task, "critical");
                        totalNotifications++;
                        
                        // Send admin notification for critical tasks
                        if (_settings.AdminEmails.Length > 0)
                        {
                            await SendAdminAlertAsync(task, scope.ServiceProvider);
                        }
                    }
                }

                if (_settings.EnableConsoleLogging)
                {
                    _logger.LogInformation("Deadline check completed: {EarlyCount} early, {UrgentCount} urgent, {CriticalCount} critical, Total notifications: {TotalCount}", 
                        earlyWarningTasks.Count, urgentWarningTasks.Count, criticalWarningTasks.Count, totalNotifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in deadline notification check");
            }
        }

        private async Task<bool> ShouldSendNotificationAsync(Models.TaskItem task, int hoursThreshold)
        {
            if (task.AssignedUser == null) return false;
            
            var timeRemaining = task.Deadline - DateTime.UtcNow;
            var hoursRemaining = (int)timeRemaining.TotalHours;
            
            // Only send notification if task is within the threshold and not already completed
            return hoursRemaining <= hoursThreshold && hoursRemaining > 0 && task.Status != Models.TStatus.Done;
        }

        private async Task SendAdminAlertAsync(Models.TaskItem task, IServiceProvider serviceProvider)
        {
            try
            {
                var mailService = serviceProvider.GetRequiredService<IMailService>();
                
                foreach (var adminEmail in _settings.AdminEmails)
                {
                    var mailData = new Models.Mail
                    {
                        EmailToId = adminEmail,
                        EmailToName = "Administrator",
                        EmailSubject = $"🚨 CRITICAL: Task '{task.Title}' is overdue!",
                        EmailBody = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                                <h2 style='color: #d32f2f;'>🚨 CRITICAL TASK ALERT</h2>
                                <p>A task is critically overdue and requires immediate attention!</p>
                                
                                <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #d32f2f;'>
                                    <h3>Task Details:</h3>
                                    <p><strong>Title:</strong> {task.Title}</p>
                                    <p><strong>Description:</strong> {task.Description}</p>
                                    <p><strong>Priority:</strong> {task.Priority}</p>
                                    <p><strong>Status:</strong> {task.Status}</p>
                                    <p><strong>Deadline:</strong> {task.Deadline:MMM dd, yyyy 'at' HH:mm}</p>
                                    <p><strong>Assigned To:</strong> {task.AssignedUser?.FullName ?? "Unassigned"}</p>
                                    <p><strong>Time Overdue:</strong> {(DateTime.UtcNow - task.Deadline).TotalHours:F1} hours</p>
                                </div>
                                
                                <p><strong>Action Required:</strong> Please contact the assigned user immediately and escalate if necessary.</p>
                                <p>Best regards,<br/>TaskFlow Pro System</p>
                            </div>"
                    };

                    await mailService.SendMail(mailData);
                }
                
                _logger.LogWarning("Admin alert sent for critical task {TaskId}", task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin alert for critical task {TaskId}", task.Id);
            }
        }
    }
}
