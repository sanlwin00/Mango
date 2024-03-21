using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService _baseService;
        public CouponService(IBaseService baseService) 
        { 
            _baseService = baseService;
        }
        public async Task<ResponseDto?> CreateCouponAsync(CouponDto couponDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon",
                Data = couponDto
            });
        }

        public async Task<ResponseDto?> DeleteCouponAsync(string CouponId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.DELETE,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon/" + CouponId
            });
        }

        public async Task<ResponseDto?> GetAllCouponAsync()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon"
            });
        }

        public async Task<ResponseDto?> GetCouponAsync(string couponCode)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon/GetByCode/" + couponCode
            });
        }

        public async Task<ResponseDto?> GetCouponbyIdAsync(string CouponId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon/" + CouponId
            });
        }

        public async Task<ResponseDto?> UpdateCouponAsync(CouponDto couponDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.PUT,
                Url = StaticData.CouponApiBaseUrl + "/api/coupon",
                Data = couponDto
            });
        }
    }
}
