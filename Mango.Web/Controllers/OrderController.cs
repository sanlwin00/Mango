using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Services;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult OrderIndex()
        {
            return View();
        }
		public async Task<IActionResult> OrderDetail(int orderId)
		{
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

			var response = await _orderService.GetOrder(orderId);
            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            }
            if(User.IsInRole(StaticData.Roles.ADMIN.ToString()) || userId == orderHeaderDto.UserId)
            {
				return View(orderHeaderDto);
			}
			return NotFound();
		}
        public async Task<IActionResult> OrderReady(int orderId)
        {            
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            var response = await _orderService.UpdateOrderStatus(orderId, StaticData.OrderStatus.ReadyForPickup.ToString());

            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                if (User.IsInRole(StaticData.Roles.ADMIN.ToString()) || userId == orderHeaderDto.UserId)
                {
                    TempData["success"] = "Status updated successfully!";                    
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                TempData["error"] = $"Failed to update. {response?.Message}";
            }

            return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
        }
        public async Task<IActionResult> OrderComplete(int orderId)
        {
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            var response = await _orderService.UpdateOrderStatus(orderId, StaticData.OrderStatus.Completed.ToString());

            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                if (User.IsInRole(StaticData.Roles.ADMIN.ToString()) || userId == orderHeaderDto.UserId)
                {
                    TempData["success"] = "Status updated successfully!";
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                TempData["error"] = $"Failed to update. {response?.Message}";
            }

            return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
        }
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            var response = await _orderService.UpdateOrderStatus(orderId, StaticData.OrderStatus.Cancelled.ToString());

            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                if (User.IsInRole(StaticData.Roles.ADMIN.ToString()) || userId == orderHeaderDto.UserId)
                {
                    TempData["success"] = "Status updated successfully!";
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                TempData["error"] = $"Failed to update. {response?.Message}";
            }

            return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
        }
        [HttpGet]
        public IActionResult GetAllOrders([FromQuery]string status)
        {
			IEnumerable<OrderHeaderDto> orders = new List<OrderHeaderDto>();
            string userId = string.Empty;
            if(!User.IsInRole(StaticData.Roles.ADMIN.ToString()))
            {
                userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }
            ResponseDto response = _orderService.GetAllOrders(userId).GetAwaiter().GetResult();
            if(response != null && response.IsSuccess)
            {
                orders = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
                if (status != null && Enum.IsDefined(typeof(StaticData.OrderStatus), status))
                {
                    orders = orders.Where(x => x.Status == status);
                }
            }
            return Json(new { data = orders });
        }
    }
}
