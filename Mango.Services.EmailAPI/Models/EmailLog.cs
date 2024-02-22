namespace Mango.Services.EmailAPI.Models
{
    public class EmailLog
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string Message { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
