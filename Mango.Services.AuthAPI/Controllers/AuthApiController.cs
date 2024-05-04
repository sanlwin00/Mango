using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        protected ResponseDto _response;
        private readonly IAuthService _authService;        
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        public AuthApiController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
        {
            _authService = authService;
            _response = new();
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDto dto)
        {
            string error = await _authService.Register(dto);
            if(error != "")
            {
                _response.IsSuccess = false;  
                _response.Result = dto;    
                _response.Message = error;          
                return BadRequest(_response);                
            }
            else
            {
                _response.IsSuccess = true;   
                _response.Result = await _authService.GetUser(dto.Email);

                string queueName = _configuration.GetValue<string>("MessageQueueNames:RegisterEmailQueue");
                _messageBus.PublishMessageAsync(dto, queueName);

                return Ok(_response);
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationDto dto)
        {
            var result = await _authService.AssignRole(dto.Email, dto.Role);
            if (result == false)
            {
                _response.IsSuccess = false;
                _response.Result = dto;
                _response.Message = "Role assignment failed.";
                return BadRequest(_response);
            }
            else
            {
                _response.IsSuccess = true;
                _response.Result = await _authService.GetUser(dto.Email);
                return Ok(_response);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var loginResponseDto = await _authService.Login(dto);
            if (loginResponseDto.User != null)
            {
                _response.IsSuccess = true;
                _response.Result = loginResponseDto;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;             
                _response.Message = "Username or password is incorrect.";                           
                return BadRequest(_response);  
            }
        }
    }
}
