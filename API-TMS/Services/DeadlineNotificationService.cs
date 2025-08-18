using API_TMS.Repository.Interface;

namespace API_TMS.Services
{
    public class DeadlineNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeadlineNotificationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public DeadlineNotificationService(
            IServiceProvider serviceProvider,
            ILogger<DeadlineNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckDeadlinesAndNotifyAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskItemRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var tasksDueSoon = await taskRepository.GetDueSoonAsync(24);
            
            foreach (var task in tasksDueSoon)
            {
                try
                {
                    await notificationService.SendTaskDeadlineNotificationAsync(task);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending deadline notification for task {TaskId}", task.Id);
                }
            }

            _logger.LogInformation("Checked {TaskCount} tasks for deadline notifications", tasksDueSoon.Count);
        }
    }
}
