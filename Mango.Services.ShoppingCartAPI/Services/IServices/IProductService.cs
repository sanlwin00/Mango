using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Services.IServices
{
    public interface IProductService
    {
        Task<ResponseDto> GetProductbyIdAsync(int id);
    }
}
