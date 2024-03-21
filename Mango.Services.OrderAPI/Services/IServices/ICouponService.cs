using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI.Services.IServices
{
    public interface ICouponService
    {
        Task<ResponseDto> GetCouponAsync(string couponCode);
    }
}
