using API_TMS.Models;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.AssignedUser)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            var adminUser = new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                Password = "$2a$11$piq0d4yxGqdHMnxJr3VfGehcUE9S5zI0v/ysQrtvMRN/8gk4FgU8K",
                Role = "Admin",
                PhoneNumber = "0000000000",
                CreatedAt = DateTime.Now
            };

            var user1 = new User
            {
                Id = 2,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "$2a$11$VQu8FZgLhfrnltbOmfefWeyOMbI0Q0xMmaDdzBNjSZyNoG0wzP7fS",
                Role = "User",
                PhoneNumber = "1111111111",
                CreatedAt = DateTime.Now
            };

            var task1 = new TaskItem
            {
                Id = 1,
                Title = "Initial Setup",
                Description = "Setup development environment",
                Deadline = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Priority = TaskPriority.Low,
                Status = TStatus.ToDo,
                AssignedUserId = user1.Id,
                CreatedAt = DateTime.Now
            };

            var templates = new[]
            {
                new EmailTemplate
                {
                    Id = 1,
                    TemaplteName = "Login",
                    FromAddress = "mehreenm.imran27@gmail.com",
                    Subject = "Welcome to Task Tracker",
                    Body = "Hello {{name}}, you have successfully logged in."
                },
                new EmailTemplate
                {
                    Id = 2,
                    TemaplteName = "TaskAssignment",
                    FromAddress = "mehreenm.imran27@gmail.com",
                    Subject = "New Task Assigned",
                    Body = "Hello {{name}}, a new task '{{task}}' has been assigned to you."
                },
                new EmailTemplate
                {
                    Id = 3,
                    TemaplteName = "Reminder",
                    FromAddress = "mehreenm.imran27@gmail.com",
                    Subject = "Task Deadline Approaching",
                    Body = "Reminder: Task '{{task}}' is due in less than 24 hours."
                },
                new EmailTemplate
                {   
                    Id = 4,
                    TemaplteName = "Announcement",
                    FromAddress = "mehreenm.imran27@gmail.com",
                    Subject = "Important Announcement",
                    Body = "Dear team, please be informed: {{announcement}}"
                }
            };

            var announcement1 = new Announcement
            {
                Id = 1,
                Subject = "Welcome to TMS",
                Body = "Welcome to our new Task Management System!",
                CreatedAt = DateTime.UtcNow,
            };

            modelBuilder.Entity<User>().HasData(adminUser, user1);
            modelBuilder.Entity<System.Threading.Tasks.Task>().HasData(task1);
            modelBuilder.Entity<Announcement>().HasData(announcement1);
            modelBuilder.Entity<EmailTemplate>().HasData(templates);
        }
    }
}