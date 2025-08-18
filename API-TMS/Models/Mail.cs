namespace API_TMS.Models
{
    public class Mail
    {
        public required string EmailToId { get; set; }
        public required string EmailToName { get; set; }
        public required string EmailSubject { get; set; }
        public required string EmailBody { get; set; }
    }
}
