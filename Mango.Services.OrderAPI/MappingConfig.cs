using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMappings()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CartHeaderDto, OrderHeaderDto>()
                    .ForMember(dest => dest.OrderTotal, x => x.MapFrom(src => src.CartTotal))
                    .ReverseMap();

                config.CreateMap<CartDetailDto, OrderDetailDto>()
                    .ForMember(dest => dest.ProductName, x => x.MapFrom(src => src.Product.Name))
                    .ForMember(dest => dest.Price, x => x.MapFrom(src => src.Product.Price));
                
                config.CreateMap<OrderHeaderDto, OrderHeader>().ReverseMap();
                config.CreateMap<OrderDetailDto, OrderDetail>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}



