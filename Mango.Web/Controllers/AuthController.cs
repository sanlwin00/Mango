using Mango.Web.Models.Dto;
using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) 
        {
            _authService = authService;
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
                    RedirectToAction("Index","Home");
                }
                else
                {
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
            RegistrationDto registrationDto = new();
            return View(registrationDto);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }
    }
}
