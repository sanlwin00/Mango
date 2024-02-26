using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models.Dto;
using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Mango.Services.OrderAPI.Utility;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private ResponseDto _response;
        private IMapper _mapper;
        private IProductService _productService;
        public OrderApiController(AppDbContext db, IMapper mapper, IConfiguration configuration, IProductService productService)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
            _configuration = configuration;
            _productService = productService;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> Post([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto header = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                header.OrderDate = DateTime.Now;
                header.Status = StaticData.OrderStatus.Pending.ToString();
                header.OrderDetails = _mapper.Map<IEnumerable<OrderDetailDto>>(cartDto.CartDetails);
                
                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(header)).Entity;
                await _db.SaveChangesAsync();

                header.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = header;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

    }
}

