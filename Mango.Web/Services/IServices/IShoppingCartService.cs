using Mango.Web.Models;
using Mango.Web.Models.Dto;

namespace Mango.Web.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<ResponseDto> GetCartByUserAsync(string userId);
        Task<ResponseDto> UpsertCartAsync(CartDto cartDto);
        Task<ResponseDto> SetCouponAsync(CartDto cartDto);
        Task<ResponseDto> RemoveFromCart(int cartDetailId);
    }
}
