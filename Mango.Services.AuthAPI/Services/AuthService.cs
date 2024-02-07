using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.CouponAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDto?> GetUser(string userName)
    {
        var user = await _db.ApplicationUsers.FirstAsync(x => x.UserName.ToLower() == userName.ToLower());
        if (user != null)
        {
            UserDto userDto = new(){
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
            return userDto;
        }
        return null;
    }

    public async Task<LoginResponseDto?> Login(LoginRequestDto loginRequestDto)
    {
        ApplicationUser user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginRequestDto.UserName.ToLower());
        bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
        if (user != null && isValid == true)
        {
            UserDto userDto = new(){
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
            return new LoginResponseDto(){ User = userDto, Token = ""};
        }
        else
        {
            return new LoginResponseDto(){ User = null, Token=""};
        }
    }

    public async Task<string> Register(RegistrationDto registrationDto)
    {
        ApplicationUser user = new(){
            UserName = registrationDto.Email,
            Email = registrationDto.Email,
            NormalizedEmail = registrationDto.Email.ToUpper(),
            Name = registrationDto.Name,
            PhoneNumber = registrationDto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, registrationDto.Password);
        
        if (result.Succeeded)
        {            
            return "";
        }
        else
        {
            return result.Errors.FirstOrDefault()?.Description;
        }
    }
}
