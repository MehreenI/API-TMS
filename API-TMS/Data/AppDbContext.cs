using API_TMS.Models;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Deadline).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.AssignedUser)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(e => e.AssignedUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Announcement>(entity =>
                {   
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TemaplteName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired();
            });
        }
    }
}