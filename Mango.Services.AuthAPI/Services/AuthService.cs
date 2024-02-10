using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Services.IService;
using Mango.Services.CouponAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<bool> AssignRole(string email, string roleName)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == email.ToLower());
        if (user != null)
        {
            if(!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult()) 
            { 
                //create role if it doesn't exist
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }
        return false;
    }

    public async Task<UserDto?> GetUser(string userName)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == userName.ToLower());
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
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenGenerator.GenerateToken(user, roles);
            UserDto userDto = new(){
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
            return new LoginResponseDto(){ User = userDto, Token = token};
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
