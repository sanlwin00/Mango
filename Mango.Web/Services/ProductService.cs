using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;
        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto?> GetAllProductAsync()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.ProductApiBaseUrl + "/api/product/all"
            });
        }

        public async Task<ResponseDto?> GetProductbyIdAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.GET,
                Url = StaticData.ProductApiBaseUrl + "/api/product/" + id
            });
        }
        public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.POST,
                Url = StaticData.ProductApiBaseUrl + "/api/product",
                Data = productDto,
                ContentType = StaticData.ContentType.MultipartFormData
            });
        }

        public async Task<ResponseDto?> DeleteProductAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.DELETE,
                Url = StaticData.ProductApiBaseUrl + "/api/product/delete/" + id
            });
        }        

        public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticData.ApiType.PUT,
                Url = StaticData.ProductApiBaseUrl + "/api/product",
                Data = productDto,
                ContentType = StaticData.ContentType.MultipartFormData
            });
        }
    }
}
