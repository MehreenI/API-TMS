using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface IAnnouncementRepository
    {
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<Announcement?> GetByIdAsync(int id);
        Task<Announcement> CreateAsync(Announcement announcement);
        Task<Announcement> UpdateAsync(Announcement announcement);
        Task<bool> DeleteAsync(int id);
    }
}
