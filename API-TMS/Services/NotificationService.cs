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

        public async Task SendTaskDeadlineNotificationAsync(TaskItem task, string notificationType = "default")
        {
            try
            {
                if (task.AssignedUser == null) return;

                var timeRemaining = task.Deadline - DateTime.UtcNow;
                var hoursRemaining = (int)timeRemaining.TotalHours;
                var isOverdue = timeRemaining < TimeSpan.Zero;

                var (subjectPrefix, alertColor, priorityIcon) = GetNotificationStyle(notificationType, isOverdue, hoursRemaining);

                var mailData = new Mail
                {
                    EmailToId = task.AssignedUser.Email,
                    EmailToName = task.AssignedUser.FullName,
                    EmailSubject = $"{subjectPrefix}: {task.Title}",
                    EmailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Task Deadline Alert - Taskify</title>
                        </head>
                        <body style='margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f5f5; padding: 20px 0;'>
                                <tr>
                                    <td align='center'>
                                        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                                            
                                            <!-- Header -->
                                            <tr>
                                                <td style='background-color: #dc2626; padding: 30px 40px; text-align: center;'>
                                                    <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold; letter-spacing: 1px;'>TASKIFY</h1>
                                                    <p style='color: #fecaca; margin: 8px 0 0 0; font-size: 14px; font-weight: 500;'>Professional Task Management System</p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Content -->
                                            <tr>
                                                <td style='padding: 40px;'>
                                                    <div style='text-align: center; margin-bottom: 30px;'>
                                                        <div style='background-color: {alertColor}; color: #ffffff; padding: 12px 24px; border-radius: 25px; display: inline-block; font-weight: bold; font-size: 16px;'>
                                                            {priorityIcon} Task Deadline Alert
                                                        </div>
                                                    </div>
                                                    
                                                    <h2 style='color: #1f2937; margin: 0 0 20px 0; font-size: 24px;'>Hello {task.AssignedUser.FullName},</h2>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                                        This is an important reminder regarding your assigned task. Please review the details below and take necessary action.
                                                    </p>
                                                    
                                                    <!-- Task Details Card -->
                                                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; margin: 25px 0;'>
                                                        <tr>
                                                            <td style='padding: 25px;'>
                                                                <h3 style='color: #111827; margin: 0 0 20px 0; font-size: 18px; font-weight: bold; border-bottom: 2px solid #dc2626; padding-bottom: 10px;'>Task Details</h3>
                                                                
                                                                <table width='100%' cellpadding='8' cellspacing='0'>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold; width: 120px; vertical-align: top;'>Title:</td>
                                                                        <td style='color: #1f2937; font-weight: 600;'>{task.Title}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold; vertical-align: top;'>Description:</td>
                                                                        <td style='color: #4b5563; line-height: 1.5;'>{task.Description}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Priority:</td>
                                                                        <td style='color: #dc2626; font-weight: bold;'>{task.Priority}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Status:</td>
                                                                        <td style='color: #059669; font-weight: 600;'>{task.Status}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Deadline:</td>
                                                                        <td style='color: #dc2626; font-weight: bold; font-size: 16px;'>{task.Deadline:MMMM dd, yyyy 'at' HH:mm}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Time Status:</td>
                                                                        <td style='color: {(isOverdue ? "#dc2626" : "#059669")}; font-weight: bold; font-size: 16px;'>{(isOverdue ? "⚠️ OVERDUE" : $"✅ {hoursRemaining} hours remaining")}</td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    
                                                    <div style='background-color: #fef3c7; border: 1px solid #f59e0b; border-radius: 6px; padding: 15px; margin: 25px 0;'>
                                                        <p style='margin: 0; color: #92400e; font-weight: 600; font-size: 14px;'>
                                                            <strong>⚠️ Action Required:</strong> Please ensure this task is completed before the deadline to maintain project timeline.
                                                        </p>
                                                    </div>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 25px 0 0 0;'>
                                                        Thank you for your attention to this matter. If you have any questions or need assistance, please contact our support team.
                                                    </p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Footer -->
                                            <tr>
                                                <td style='background-color: #111827; padding: 30px 40px; text-align: center;'>
                                                    <h3 style='color: #ffffff; margin: 0 0 10px 0; font-size: 18px;'>Best Regards,</h3>
                                                    <p style='color: #d1d5db; margin: 0 0 15px 0; font-size: 16px; font-weight: 600;'>Taskify Team</p>
                                                    <div style='border-top: 1px solid #374151; padding-top: 15px; margin-top: 15px;'>
                                                        <p style='color: #9ca3af; margin: 0; font-size: 14px;'>Support: mehreenm.imran27@gmail.com</p>
                                                        <p style='color: #6b7280; margin: 5px 0 0 0; font-size: 12px;'>© 2025 Taskify. All rights reserved.</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                        </html>"
                };

                await _mailService.SendMail(mailData);
                _logger.LogInformation("Deadline notification ({Type}) sent for task {TaskId} to {Email}",
                    notificationType, task.Id, task.AssignedUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending deadline notification for task {TaskId}", task.Id);
            }
        }

        private (string subject, string alertColor, string icon) GetNotificationStyle(string notificationType, bool isOverdue, int hoursRemaining)
        {
            return notificationType switch
            {
                "critical" => (
                    "🚨 CRITICAL ALERT",
                    "#dc2626",
                    "🚨"
                ),
                "urgent" => (
                    "⚠️ URGENT NOTICE",
                    "#ea580c",
                    "⚠️"
                ),
                "early" => (
                    "📅 EARLY REMINDER",
                    "#059669",
                    "📅"
                ),
                _ => (
                    "📋 TASK REMINDER",
                    "#4f46e5",
                    "📋"
                )
            };
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
                    EmailSubject = $"New Task Assignment - {task.Title}",
                    EmailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>New Task Assignment - Taskify</title>
                        </head>
                        <body style='margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f5f5; padding: 20px 0;'>
                                <tr>
                                    <td align='center'>
                                        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                                            
                                            <!-- Header -->
                                            <tr>
                                                <td style='background-color: #dc2626; padding: 30px 40px; text-align: center;'>
                                                    <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold; letter-spacing: 1px;'>TASKIFY</h1>
                                                    <p style='color: #fecaca; margin: 8px 0 0 0; font-size: 14px; font-weight: 500;'>Professional Task Management System</p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Content -->
                                            <tr>
                                                <td style='padding: 40px;'>
                                                    <div style='text-align: center; margin-bottom: 30px;'>
                                                        <div style='background-color: #059669; color: #ffffff; padding: 12px 24px; border-radius: 25px; display: inline-block; font-weight: bold; font-size: 16px;'>
                                                            📋 New Task Assignment
                                                        </div>
                                                    </div>
                                                    
                                                    <h2 style='color: #1f2937; margin: 0 0 20px 0; font-size: 24px;'>Hello {task.AssignedUser.FullName},</h2>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                                        A new task has been assigned to you in Taskify. Please review the task details below and begin working on it at your earliest convenience.
                                                    </p>
                                                    
                                                    <!-- Task Details Card -->
                                                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; margin: 25px 0;'>
                                                        <tr>
                                                            <td style='padding: 25px;'>
                                                                <h3 style='color: #111827; margin: 0 0 20px 0; font-size: 18px; font-weight: bold; border-bottom: 2px solid #dc2626; padding-bottom: 10px;'>Task Assignment Details</h3>
                                                                
                                                                <table width='100%' cellpadding='8' cellspacing='0'>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold; width: 120px; vertical-align: top;'>Task Title:</td>
                                                                        <td style='color: #1f2937; font-weight: 600; font-size: 16px;'>{task.Title}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold; vertical-align: top;'>Description:</td>
                                                                        <td style='color: #4b5563; line-height: 1.5;'>{task.Description}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Priority Level:</td>
                                                                        <td style='color: #dc2626; font-weight: bold;'>{task.Priority}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Current Status:</td>
                                                                        <td style='color: #059669; font-weight: 600;'>{task.Status}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Due Date:</td>
                                                                        <td style='color: #dc2626; font-weight: bold; font-size: 16px;'>{task.Deadline:MMMM dd, yyyy 'at' HH:mm}</td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    
                                                    <div style='background-color: #dbeafe; border: 1px solid #3b82f6; border-radius: 6px; padding: 15px; margin: 25px 0;'>
                                                        <p style='margin: 0; color: #1e40af; font-weight: 600; font-size: 14px;'>
                                                            <strong>📝 Next Steps:</strong> Please review the task requirements and update the status as you progress through completion.
                                                        </p>
                                                    </div>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 25px 0 0 0;'>
                                                        If you have any questions about this assignment or need additional resources, please don't hesitate to reach out to our support team.
                                                    </p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Footer -->
                                            <tr>
                                                <td style='background-color: #111827; padding: 30px 40px; text-align: center;'>
                                                    <h3 style='color: #ffffff; margin: 0 0 10px 0; font-size: 18px;'>Best Regards,</h3>
                                                    <p style='color: #d1d5db; margin: 0 0 15px 0; font-size: 16px; font-weight: 600;'>Taskify Team</p>
                                                    <div style='border-top: 1px solid #374151; padding-top: 15px; margin-top: 15px;'>
                                                        <p style='color: #9ca3af; margin: 0; font-size: 14px;'>Support: mehreenm.imran27@gmail.com</p>
                                                        <p style='color: #6b7280; margin: 5px 0 0 0; font-size: 12px;'>© 2025 Taskify. All rights reserved.</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                        </html>"
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
                    EmailSubject = "Welcome to Taskify - Your Account is Ready!",
                    EmailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Welcome to Taskify</title>
                        </head>
                        <body style='margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f5f5; padding: 20px 0;'>
                                <tr>
                                    <td align='center'>
                                        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                                            
                                            <!-- Header -->
                                            <tr>
                                                <td style='background-color: #dc2626; padding: 30px 40px; text-align: center;'>
                                                    <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold; letter-spacing: 1px;'>TASKIFY</h1>
                                                    <p style='color: #fecaca; margin: 8px 0 0 0; font-size: 14px; font-weight: 500;'>Professional Task Management System</p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Content -->
                                            <tr>
                                                <td style='padding: 40px;'>
                                                    <div style='text-align: center; margin-bottom: 30px;'>
                                                        <div style='background-color: #059669; color: #ffffff; padding: 12px 24px; border-radius: 25px; display: inline-block; font-weight: bold; font-size: 16px;'>
                                                            🎉 Welcome to Taskify!
                                                        </div>
                                                    </div>
                                                    
                                                    <h2 style='color: #1f2937; margin: 0 0 20px 0; font-size: 24px;'>Hello {user.FullName},</h2>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                                        Congratulations! Your Taskify account has been successfully created. We're excited to have you join our professional task management platform.
                                                    </p>
                                                    
                                                    <!-- Account Details Card -->
                                                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; margin: 25px 0;'>
                                                        <tr>
                                                            <td style='padding: 25px;'>
                                                                <h3 style='color: #111827; margin: 0 0 20px 0; font-size: 18px; font-weight: bold; border-bottom: 2px solid #dc2626; padding-bottom: 10px;'>Your Account Information</h3>
                                                                
                                                                <table width='100%' cellpadding='8' cellspacing='0'>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold; width: 150px;'>Email Address:</td>
                                                                        <td style='color: #1f2937; font-weight: 600;'>{user.Email}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>User Role:</td>
                                                                        <td style='color: #059669; font-weight: 600;'>{user.Role}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style='color: #374151; font-weight: bold;'>Temporary Password:</td>
                                                                        <td style='background-color: #fef3c7; padding: 8px 12px; border-radius: 4px; font-family: monospace; font-weight: bold; color: #92400e; font-size: 16px;'>{tempPassword}</td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    
                                                    <div style='background-color: #fef2f2; border: 1px solid #dc2626; border-radius: 6px; padding: 20px; margin: 25px 0;'>
                                                        <h4 style='color: #dc2626; margin: 0 0 10px 0; font-size: 16px; font-weight: bold;'>🔒 Important Security Notice</h4>
                                                        <p style='margin: 0; color: #991b1b; font-weight: 600; font-size: 14px; line-height: 1.5;'>
                                                            For your account security, please change your temporary password immediately after your first login. Your account safety is our top priority.
                                                        </p>
                                                    </div>
                                                    
                                                    <div style='background-color: #f0f9ff; border: 1px solid #0ea5e9; border-radius: 6px; padding: 15px; margin: 25px 0;'>
                                                        <p style='margin: 0; color: #0c4a6e; font-weight: 600; font-size: 14px;'>
                                                            <strong>🚀 Getting Started:</strong> You can now log in to Taskify and start organizing your tasks efficiently with our comprehensive management tools.
                                                        </p>
                                                    </div>
                                                    
                                                    <p style='color: #4b5563; font-size: 16px; line-height: 1.6; margin: 25px 0 0 0;'>
                                                        Welcome aboard! If you need any assistance or have questions about using Taskify, our support team is here to help you succeed.
                                                    </p>
                                                </td>
                                            </tr>
                                            
                                            <!-- Footer -->
                                            <tr>
                                                <td style='background-color: #111827; padding: 30px 40px; text-align: center;'>
                                                    <h3 style='color: #ffffff; margin: 0 0 10px 0; font-size: 18px;'>Best Regards,</h3>
                                                    <p style='color: #d1d5db; margin: 0 0 15px 0; font-size: 16px; font-weight: 600;'>Taskify Team</p>
                                                    <div style='border-top: 1px solid #374151; padding-top: 15px; margin-top: 15px;'>
                                                        <p style='color: #9ca3af; margin: 0; font-size: 14px;'>Support: mehreenm.imran27@gmail.com</p>
                                                        <p style='color: #6b7280; margin: 5px 0 0 0; font-size: 12px;'>© 2025 Taskify. All rights reserved.</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                        </html>"
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