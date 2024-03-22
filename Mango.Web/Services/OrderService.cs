using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;
        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateOrder(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.OrderApiBaseUrl + "/api/order/CreateOrder",
                Data = cartDto
            });
        }

        public async Task<ResponseDto> CreateStripeSession(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.OrderApiBaseUrl + "/api/order/CreateStripeSession",
                Data = stripeRequestDto
            });
        }

        public async Task<ResponseDto> ValidateStripeSession(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.OrderApiBaseUrl + "/api/order/ValidateStripeSession",
                Data = orderHeaderId
            });
        }
    }
}
