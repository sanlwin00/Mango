using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Services.IServices
{
    public interface ICouponService
    {
        Task<ResponseDto> GetCouponAsync(string couponCode);
    }
}
