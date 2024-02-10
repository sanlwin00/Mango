using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface IProductService
    {
        Task<ResponseDto?> GetAllProductAsync();
        Task<ResponseDto> GetProductbyIdAsync(int id);
        Task<ResponseDto> CreateProductAsync(ProductDto ProductDto);
        Task<ResponseDto> UpdateProductAsync(ProductDto ProductDto);
        Task<ResponseDto> DeleteProductAsync(int id);
    }
}
