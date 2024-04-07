using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetAllCouponAsync();
        Task<ResponseDto> GetCouponAsync(string couponCode);
        Task<ResponseDto> GetCouponbyIdAsync(string CouponId);        
        Task<ResponseDto> CreateCouponAsync(CouponDto couponDto);
        Task<ResponseDto> DeleteCouponAsync(string CouponId);
    }
}
