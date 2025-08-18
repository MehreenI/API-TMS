
using API_TMS.Models;

namespace API_TMS.Services
{
    public interface IMailService
    {
        Task<bool> SendMail(Mail Mail_Data);
    }
}
