using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private IProductService _productService;
        public CartApiController(AppDbContext db, IMapper mapper, IProductService productService)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> Get(string userId)
        {
            try
            {
                CartDto cartDto = new();
                var cartHeader = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId);
                if (cartHeader != null)
                {
                    var cartDetails = _db.CartDetails.Where(x => x.CartHeaderId == cartHeader.CartHeaderId).ToList();

                    cartDto.CartHeader = _mapper.Map<CartHeaderDto>(cartHeader);
                    cartDto.CartDetails = _mapper.Map<List<CartDetailDto>>(cartDetails);
                    foreach (var item in cartDto.CartDetails)
                    {
                        var response = await _productService.GetProductbyIdAsync(item.ProductId);
                        if (response != null && response.IsSuccess)
                        {
                            item.Product = JsonConvert.DeserializeObject<ProductDto>(response.Result?.ToString());
                            cartDto.CartHeader.CartTotal += (item.Qty * item.Product.Price);
                        }
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "Not found";
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

        [HttpPost("UpsertCart")]
        public async Task<ResponseDto> Post([FromBody] CartDto cartDto)
        {
            try
            {
                //check if there is any existing cart for the user
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    //create cart header 
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    await _db.CartHeaders.AddAsync(cartHeader);
                    await _db.SaveChangesAsync();

                    //get cartId from newly created cartHeader and assign to cartDetail object
                    cartDto.CartDetails.First().CartHeaderId = cartDto.CartHeader.CartHeaderId = cartHeader.CartHeaderId;

                    //create cart detail
                    CartDetail cartDetail = _mapper.Map<CartDetail>(cartDto.CartDetails.First());
                    _db.CartDetails.Add(cartDetail);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartDetailId = cartDetail.CartDetailId;
                }
                else
                {
                    cartDto.CartHeader.CartHeaderId = cartHeaderFromDb.CartHeaderId;

                    //cart exists, check if the same product exists in the cart. Note that First() can be used bcuz only one product can be added at a time
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(x => x.CartHeaderId == cartHeaderFromDb.CartHeaderId
                                        && x.ProductId == cartDto.CartDetails.First().ProductId);
                    if (cartDetailsFromDb == null)
                    {
                        //get cartId from existing cartHeader and assign to cartDetail object
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

                        //create cart detail 
                        CartDetail cartDetail = _mapper.Map<CartDetail>(cartDto.CartDetails.First());
                        _db.CartDetails.Add(cartDetail);
                        await _db.SaveChangesAsync();
                        cartDto.CartDetails.First().CartDetailId = cartDetail.CartDetailId;
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

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart(int cartDetailId)
        {
            try
            {
                CartDetail cartDetail = _db.CartDetails.First(x => x.CartDetailId == cartDetailId);

                int totalItemsInCart = _db.CartDetails.Count(x => x.CartHeaderId == cartDetail.CartHeaderId);

                _db.CartDetails.Remove(cartDetail);
                if (totalItemsInCart == 1)
                {
                    //delete cart header too
                    CartHeader cartHeader = _db.CartHeaders.First(x => x.CartHeaderId == cartDetail.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeader);
                }
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                //check if there is any existing cart for the user
                var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb != null)
                {
                    cartHeaderFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                    _db.Update(cartHeaderFromDb);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "Cart not found";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                //check if there is any existing cart for the user
                var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb != null)
                {
                    cartHeaderFromDb.CouponCode = string.Empty;
                    _db.Update(cartHeaderFromDb);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "Cart not found";
                }
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


