﻿using Mango.Web.Models;
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
        [HttpPost("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartDto cart)
        {
            var userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            string tmp = JsonConvert.SerializeObject(cart);
            ResponseDto ? response = await _cartService.SetCouponAsync(cart);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon applied!";
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction("CartIndex");
        }
    }
}
