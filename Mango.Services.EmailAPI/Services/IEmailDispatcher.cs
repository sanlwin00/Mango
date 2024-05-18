namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailDispatcher
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
    }
}
