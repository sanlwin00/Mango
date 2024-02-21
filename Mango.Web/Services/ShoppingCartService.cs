using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IBaseService _baseService;
        public ShoppingCartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> EmailCart(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.ShoppingCartApiBaseUrl + "/api/cart/EmailCart",
                Data = cartDto
            });
        }

        public async Task<ResponseDto> GetCartByUserAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.ShoppingCartApiBaseUrl + "/api/cart/GetCart/" + userId
            });
        }

        public async Task<ResponseDto> RemoveFromCart(int cartDetailId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.ShoppingCartApiBaseUrl + "/api/cart/RemoveCart",
                Data = cartDetailId
            });
        }

        public async Task<ResponseDto> SetCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.ShoppingCartApiBaseUrl + "/api/cart/SetCoupon",
                Data = cartDto
            });
        }

        public async Task<ResponseDto> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.ShoppingCartApiBaseUrl + "/api/cart/UpsertCart",
                Data = cartDto
            });
        }
    }
}
