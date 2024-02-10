using Mango.Web.Models.Dto;
using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider) 
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }
        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto obj = new();
            return View(obj);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto? obj)
        {
            try{
                ResponseDto? response = await _authService.LoginAsync(obj);
                if (response != null && response.IsSuccess)
                {
                    LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(response.Result));
                    _tokenProvider.SetToken(loginResponse.Token);
                    await SignInUser(loginResponse);
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    //ModelState.AddModelError("CustomError", response.Message);
                    TempData["error"] = response.Message;
                }
            }
            catch(Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View(obj);
        }
        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem() { Text= StaticData.Roles.ADMIN.ToString(), Value= StaticData.Roles.ADMIN.ToString() },
                new SelectListItem() { Text= StaticData.Roles.CUSTOMER.ToString(), Value= StaticData.Roles.CUSTOMER.ToString() }
            };
            ViewBag.RoleList = roleList;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationDto obj)
        {
            try
            {
                ResponseDto? response = await _authService.RegisterAsync(obj);
                if (response != null && response.IsSuccess)
                {
                    if (string.IsNullOrEmpty(obj.Role))
                        obj.Role = StaticData.Roles.CUSTOMER.ToString();
                    ResponseDto? responseRole = await _authService.AssignRoleAsync(obj);
                    if (responseRole  != null && responseRole.IsSuccess)
                    {

                        TempData["success"] = "Registration successful!";
                        return RedirectToAction(nameof(Login));
                    }                    
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem() { Text= StaticData.Roles.ADMIN.ToString(), Value= StaticData.Roles.ADMIN.ToString() },
                new SelectListItem() { Text= StaticData.Roles.CUSTOMER.ToString(), Value= StaticData.Roles.CUSTOMER.ToString() }
            };
            ViewBag.RoleList = roleList;
            return View(obj);
        }
       
        public async Task<IActionResult> Logout()
        {            
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDto responseDto)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(responseDto.Token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email
                , jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub
                , jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name
                , jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value));
            
            identity.AddClaim(new Claim(ClaimTypes.Name
                , jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role
                , jwt.Claims.FirstOrDefault(x => x.Type == "role").Value));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
        }
    }
}
