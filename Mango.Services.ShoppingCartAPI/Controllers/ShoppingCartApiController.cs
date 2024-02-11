using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/shoppingCart")]
    [ApiController]
    public class ShoppingCartApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        public ShoppingCartApiController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<CartHeader> objList = _db.Carts.ToList();
                _response.Result = _mapper.Map<IEnumerable<CartHeaderDto>>(objList); ;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                CartHeader obj = _db.Carts.First(x => x.CartId == id);                
                _response.Result = _mapper.Map<CartHeaderDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post([FromBody] CartHeaderDto shoppingCartDto)
        {
            try
            {
                CartHeader obj = _mapper.Map<CartHeader>(shoppingCartDto);
                _db.Carts.Add(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CartHeaderDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] CartHeaderDto shoppingCartDto)
        {
            try
            {
                CartHeader obj = _mapper.Map<CartHeader>(shoppingCartDto);
                _db.Carts.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CartHeaderDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
                CartHeader obj = _db.Carts.First(x => x.CartId == id);
                _db.Carts.Remove(obj);
                _db.SaveChanges();                
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


