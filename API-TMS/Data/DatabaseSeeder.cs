using API_TMS.Models;

namespace API_TMS.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Users.Any())

                return;

            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@tms.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                PhoneNumber = "1234567890",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            var regularUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@tms.com",
                Password = BCrypt.Net.BCrypt.HashPassword("User123!"),
                PhoneNumber = "0987654321",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            var janeUser = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@tms.com",
                Password = BCrypt.Net.BCrypt.HashPassword("User123!"),
                PhoneNumber = "5555555555",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, regularUser, janeUser);
            await context.SaveChangesAsync();

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Setup Development Environment",
                    Description = "Install and configure all necessary development tools and dependencies for the project.",
                    Deadline = DateTime.UtcNow.AddDays(2),
                    Priority = TaskPriority.High,
                    Status = TStatus.InProgress,
                    AssignedUserId = regularUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Design Database Schema",
                    Description = "Create the database schema design with proper relationships and constraints.",
                    Deadline = DateTime.UtcNow.AddDays(5),
                    Priority = TaskPriority.Medium,
                    Status = TStatus.ToDo,
                    AssignedUserId = janeUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Implement Authentication System",
                    Description = "Develop JWT-based authentication system with role-based access control.",
                    Deadline = DateTime.UtcNow.AddDays(7),
                    Priority = TaskPriority.High,
                    Status = TStatus.ToDo,
                    AssignedUserId = regularUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Create API Documentation",
                    Description = "Write comprehensive API documentation using Swagger/OpenAPI.",
                    Deadline = DateTime.UtcNow.AddDays(3),
                    Priority = TaskPriority.Low,
                    Status = TStatus.Done,
                    AssignedUserId = janeUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Setup CI/CD Pipeline",
                    Description = "Configure continuous integration and deployment pipeline for automated testing and deployment.",
                    Deadline = DateTime.UtcNow.AddDays(10),
                    Priority = TaskPriority.Medium,
                    Status = TStatus.ToDo,
                    AssignedUserId = regularUser.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.TaskItems.AddRange(tasks);
            await context.SaveChangesAsync();
        }
    }
}
