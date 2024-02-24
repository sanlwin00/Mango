using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendCartEmail(CartDto cartDto);
        Task<bool> SendRegistrationEmail(RegistrationDto cartDto);
    }
}
