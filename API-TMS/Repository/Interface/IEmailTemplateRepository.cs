using API_TMS.Models;

namespace API_TMS.Repository.Interface
{
    public interface IEmailTemplateRepository
    {

        Task<IEnumerable<EmailTemplate>> GetAllAsync();
        Task<EmailTemplate> UpdateAsync(EmailTemplate emailTemplate);
    }
}
