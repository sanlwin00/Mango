using Mango.Web.Controllers;
using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Middleware
{
	public class CartItemCountActionFilter : IAsyncActionFilter
    {
        private readonly IShoppingCartService _shoppingCart;
        public CartItemCountActionFilter(IShoppingCartService shoppingCart)
        {
            _shoppingCart = shoppingCart;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
			context.HttpContext.Items["cartItemCount"] = 0;

			if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    //check cartItemCount in cache
                    var cacheKey = $"cartItemCount-{userId}";
					var itemCount = context.HttpContext.Session.GetInt32(cacheKey);

					if (!itemCount.HasValue || context.Controller is CartController)
					{
						// Fetch the count from the database if the Session is empty or refresh if user executes any action in CartController
						ResponseDto? responseDto = await _shoppingCart.GetCartByUserAsync(userId);
						if (responseDto != null && responseDto.IsSuccess)
						{
							CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(responseDto.Result));
							if (cartDto != null)
							{
								itemCount = cartDto.CartDetails.Count();
							}
                           
						}
                        if (!itemCount.HasValue)
                        {
                            itemCount = 0;
                        }
                        context.HttpContext.Session.SetInt32(cacheKey, itemCount.Value);
					}

					context.HttpContext.Items["cartItemCount"] = itemCount.Value;
                }
            }

            await next();
        }
    }

}
