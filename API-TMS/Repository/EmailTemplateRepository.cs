using API_TMS.Data;
using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API_TMS.Repository
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly AppDbContext _db;
        public EmailTemplateRepository(AppDbContext db) => _db = db;

        public async Task<EmailTemplate?> GetByTypeAsync(string emailType)
        {
            return await _db.EmailTemplates.AsNoTracking()
                           .FirstOrDefaultAsync(t => t.TemplateName == emailType);
        }
        public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
        {
            return await _db.EmailTemplates.ToListAsync();
        }

        public async Task<EmailTemplate?> UpdateAsync(EmailTemplate emailTemplate)
        {
            var existingTemplate = await _db.EmailTemplates.FindAsync(emailTemplate.Id);

            if (existingTemplate == null)
                return null;

            existingTemplate.FromAddress = emailTemplate.FromAddress;
            existingTemplate.Subject = emailTemplate.Subject;
            existingTemplate.Body = emailTemplate.Body;

            _db.EmailTemplates.Update(existingTemplate);
            await _db.SaveChangesAsync();

            return existingTemplate;
        }

    }
}
