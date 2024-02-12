using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMappings()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CartHeaderDto, CartHeader>();
                config.CreateMap<CartHeader,  CartHeaderDto>();
                config.CreateMap<CartDetailDto, CartDetail>();
                config.CreateMap<CartDetail, CartDetailDto>();                
            });
            return mappingConfig;
        }
    }
}


