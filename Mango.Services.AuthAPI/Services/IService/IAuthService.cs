using Mango.Services.AuthAPI.Models.Dto;

namespace Mango.Services.AuthAPI;

public interface IAuthService
{
    Task<string> Register(RegistrationDto registrationDto);
    Task<LoginResponseDto?> Login(LoginRequestDto loginRequestDto);
    Task<UserDto?> GetUser(string userName);
}
