using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IShoppingCartService _cartService;
        public CartController(IShoppingCartService cartService) 
        {
            _cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            var userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            ResponseDto? response = await _cartService.GetCartByUserAsync(userId);
            if (response != null && response.IsSuccess)
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return View(cartDto);
            }

            return View(new CartDto());
        }
        [Authorize]
        public async Task<IActionResult> RemoveCart(int cartDetailId)
        {
            ResponseDto? response = await _cartService.RemoveFromCart(cartDetailId);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Item removed.";
            }

            return RedirectToAction("CartIndex");
        }

        [Authorize]
        [HttpPost("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartDto cart)
        {
            string tmp = JsonConvert.SerializeObject(cart);
            ResponseDto ? response = await _cartService.SetCouponAsync(cart);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon applied! " + response.Message;
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction("CartIndex");
        }
        [Authorize]
        [HttpPost("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon(CartDto cart)
        {
            string tmp = JsonConvert.SerializeObject(cart);
            cart.CartHeader.CouponCode = string.Empty;
            ResponseDto? response = await _cartService.SetCouponAsync(cart);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon removed!";
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction("CartIndex");
        }

        [Authorize]
        [HttpPost("EmailCart")]
        public async Task<IActionResult> EmailCart(CartDto cart)
        {            
            ResponseDto? response = await _cartService.GetCartByUserAsync(cart.CartHeader.UserId);
            if (response != null && response.IsSuccess)
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                cartDto.CartHeader.Email = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault().Value;
                response = await _cartService.EmailCart(cartDto);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "You will receive the email shortly.";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction("CartIndex");
        }
    }
}
