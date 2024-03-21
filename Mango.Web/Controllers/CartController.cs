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
        private readonly IOrderService _orderService;
        public CartController(IShoppingCartService cartService, IOrderService orderService) 
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            var cartDto = await GetCartForTheLoggedInUser();
            return View(cartDto);
        }

        [Authorize]
        public async Task<IActionResult> CheckOut(CartDto cart)
        {
            var cartDto = await GetCartForTheLoggedInUser();
            return View(cartDto);
        }

        [Authorize]
        public async Task<IActionResult> PlaceOrder(CartDto cart)
        {
            var cartDto = await GetCartForTheLoggedInUser();
            cartDto.CartHeader.FirstName = cart.CartHeader.FirstName;
            cartDto.CartHeader.LastName = cart.CartHeader.LastName;
            cartDto.CartHeader.Phone = cart.CartHeader.Phone;
            cartDto.CartHeader.Email = cart.CartHeader.Email;
            
            ResponseDto? response = await _orderService.CreateOrder(cartDto);
            if (response != null && response.IsSuccess)
            {
                OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                //** get stripe session and redirect for payment
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                StripeRequestDto stripeRequest = new StripeRequestDto
                {
                    ApprovedUrl = domain + "Cart/Confirmation/" + orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "Cart/CheckOut",
                    OrderHeader = orderHeaderDto
                };
                
                var stripeResponse = await _orderService.CreateStripeSession(stripeRequest);
                if (stripeResponse != null && stripeResponse.IsSuccess)
                {
                    StripeRequestDto stripeObject = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));

                    Response.Headers.Add("Location", stripeObject.SessionUrl);
                    return new StatusCodeResult(StatusCodes.Status303SeeOther);
                }
                else
                {
                    TempData["error"] = stripeResponse?.Message;
                }
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return RedirectToAction("CartIndex");
        }

        [Authorize]
        [Route("Cart/Confirmation/{orderId}")]
        public async Task<IActionResult> Confirmation([FromRoute] int orderId)
        {
            return View(orderId);
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

        private async Task<CartDto> GetCartForTheLoggedInUser()
        {
            var userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            ResponseDto? response = await _cartService.GetCartByUserAsync(userId);
            if (response != null && response.IsSuccess)
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                cartDto.CartHeader.Email = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault().Value;
                cartDto.CartHeader.FirstName = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Name)?.FirstOrDefault().Value;
                return cartDto;
            }
            else
            {
                return new CartDto();
            }
        }
    }
}
