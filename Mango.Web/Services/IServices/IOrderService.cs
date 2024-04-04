﻿using Mango.Web.Models;
using Mango.Web.Models.Dto;

namespace Mango.Web.Services.IServices
{
    public interface IOrderService
    {
        Task<ResponseDto> CreateOrder(CartDto cartDto);
        Task<ResponseDto> CreateStripeSession(StripeRequestDto stripeRequestDto);
        Task<ResponseDto> ValidateStripeSession(int orderHeaderId);
        Task<ResponseDto> GetAllOrders(string? userId);
        Task<ResponseDto> GetOrder(int orderHeaderId);
        Task<ResponseDto> UpdateOrderStatus(int orderHeaderId, string newStatus);
    }
}
