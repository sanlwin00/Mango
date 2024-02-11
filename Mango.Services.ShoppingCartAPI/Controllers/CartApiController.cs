using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        public CartApiController(AppDbContext db, IMapper mapper)
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
                IEnumerable<CartHeader> objList = _db.CartHeaders.ToList();
                _response.Result = _mapper.Map<IEnumerable<CartDto>>(objList); ;
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
                CartHeader obj = _db.CartHeaders.First(x => x.CartHeaderId == id);                
                _response.Result = _mapper.Map<CartDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> Post([FromBody] CartDto cartDto)
        {
            try
            {
                //check if there is any existing cart for the user
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == cartDto.CartHeader.UserId);
                if(cartHeaderFromDb == null)
                {
                    //create cart header 
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    await _db.CartHeaders.AddAsync(cartHeader);
                    await _db.SaveChangesAsync();
                    
                    //get cartId from newly created cartHeader and assign to cartDetail object
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;

                    //create cart detail
                    CartDetail cartDetail = _mapper.Map<CartDetail>(cartDto.CartDetails.First());
                    _db.CartDetails.Add(cartDetail);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //cart exists, check if the same product exists in the cart. Note that First() can be used bcuz only one product can be added at a time
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(x => x.CartHeaderId == cartHeaderFromDb.CartHeaderId
                                        && x.ProductId == cartDto.CartDetails.First().ProductId);
                    if(cartDetailsFromDb == null)
                    {
                        //get cartId from existing cartHeader and assign to cartDetail object
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

                        //create cart detail 
                        CartDetail cartDetail = _mapper.Map<CartDetail>(cartDto.CartDetails.First());
                        _db.CartDetails.Add(cartDetail);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update quantity in cart detail 
                        cartDto.CartDetails.First().Qty += cartDetailsFromDb.Qty;
                        cartDto.CartDetails.First().CartDetailId = cartDetailsFromDb.CartDetailId;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;

                        CartDetail cartDetail = _mapper.Map<CartDetail>(cartDto.CartDetails.First());
                        _db.CartDetails.Update(cartDetail);
                        await _db.SaveChangesAsync();
                    }
                }
                _response.Result = cartDto;
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
        public ResponseDto Put([FromBody] CartDto shoppingCartDto)
        {
            try
            {
                CartHeader obj = _mapper.Map<CartHeader>(shoppingCartDto);
                _db.CartHeaders.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CartDto>(obj);
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
                CartHeader obj = _db.CartHeaders.First(x => x.CartHeaderId == id);
                _db.CartHeaders.Remove(obj);
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


