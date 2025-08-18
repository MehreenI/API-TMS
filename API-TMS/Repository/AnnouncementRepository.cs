using API_TMS.Data;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Repository
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _context;

        public AnnouncementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            try
            {
                return await _context.Announcements
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Announcement>();
            }
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Announcements.FindAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            try
            {
                announcement.CreatedAt = DateTime.UtcNow;
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return announcement;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Announcement> UpdateAsync(Announcement announcement)
        {
            try
            {
                var existingAnnouncement = await _context.Announcements.FindAsync(announcement.Id);
                if (existingAnnouncement == null)
                    throw new InvalidOperationException("Announcement not found");

                existingAnnouncement.Subject = announcement.Subject;
                existingAnnouncement.Body = announcement.Body;

                await _context.SaveChangesAsync();
                return existingAnnouncement;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                    return false;

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
