﻿using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI.Services.IServices
{
    public interface IProductService
    {
        Task<ResponseDto> GetProductbyIdAsync(int id);
    }
}
