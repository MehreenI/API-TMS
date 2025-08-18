namespace API_TMS.Configuration
{
    public class NotificationSettings
    {
        public int CheckIntervalHours { get; set; } = 1;
        public int EarlyWarningHours { get; set; } = 24;
        public int UrgentWarningHours { get; set; } = 6;
        public int CriticalWarningHours { get; set; } = 1;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableConsoleLogging { get; set; } = true;
        public string[] AdminEmails { get; set; } = Array.Empty<string>();
    }
}
