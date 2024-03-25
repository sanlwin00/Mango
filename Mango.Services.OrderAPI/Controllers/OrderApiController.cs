using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models.Dto;
using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Mango.Services.OrderAPI.Utility;
using Stripe;
using Stripe.Checkout;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Mango.MessageBus;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private ResponseDto _response;
        private IMapper _mapper;
        private IProductService _productService;
        private ICouponService _couponService;
        private readonly IMessageBus _messageBus;
        
        public OrderApiController(AppDbContext db, IMapper mapper, IConfiguration configuration, IProductService productService, ICouponService couponService, IMessageBus messageBus)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
            _configuration = configuration;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> Post([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto header = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                header.OrderDate = DateTime.Now;
                header.Status = StaticData.OrderStatus.Pending.ToString();
                header.OrderDetails = _mapper.Map<IEnumerable<OrderDetailDto>>(cartDto.CartDetails);

                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(header)).Entity;
                await _db.SaveChangesAsync();

                header.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = header;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
                };

                if (!stripeRequestDto.OrderHeader.CouponCode.IsNullOrEmpty())
                {
                    //get stripe coupon id and apply
                    var response = await _couponService.GetCouponAsync(stripeRequestDto.OrderHeader.CouponCode);
                    if (response != null && response.IsSuccess)
                    {
                        var couponId = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result)).CouponId;
                        var discountObj = new List<SessionDiscountOptions>
                        {
                            new SessionDiscountOptions
                            {
                                 Coupon = couponId
                            }
                        };
                        options.Discounts = discountObj;
                    }
                }

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var stripeLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long?)(item.Price * 100),
                            Currency = "cad",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            },
                        },
                        Quantity = item.Qty
                    };
                    options.LineItems.Add(stripeLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);

                stripeRequestDto.SessionUrl = session.Url;

                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();

                _response.Result = stripeRequestDto;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody]int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

                //get paymentInten from Stripe
                var service = new SessionService();
                Session session = service.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent != null && paymentIntent.Status == "succeeded")
                {
                    //payment successful
                    orderHeader.Status = StaticData.OrderStatus.Approved.ToString();
                    _db.Update(orderHeader);
                    await _db.SaveChangesAsync();

                    //log reward points in servicebus
                    RewardDto rewardDto = new()
                    {
                        OrderId = orderHeaderId,
                        RewardPoint = Convert.ToInt32(orderHeader.OrderTotal),
                        RewardDate = orderHeader.OrderDate,
                        UserId = orderHeader.UserId
                    };

                    string topicName = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedTopic");
                    await _messageBus.PublishMessage(rewardDto, topicName);

                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = paymentIntent.Status;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}

