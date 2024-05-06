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
using Serilog;

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
                Log.Information("## Creating order for user {UserId}", cartDto.CartHeader.UserId);
                Log.Debug("{@CartDto}", cartDto);
                OrderHeaderDto header = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                header.OrderDate = DateTime.Now;
                header.Status = StaticData.OrderStatus.Pending.ToString();
                header.OrderDetails = _mapper.Map<IEnumerable<OrderDetailDto>>(cartDto.CartDetails);

                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(header)).Entity;
                await _db.SaveChangesAsync();
                Log.Information("## Order created. Id:{OrderId}", orderCreated.OrderHeaderId);
                Log.Debug("{@OrderHeader}", orderCreated);
                header.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = header;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}", ex);
            }
            return _response;
        }
        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            try
            {
                Log.Information("## Updating order {Id} with status {NewStatus}", orderId, newStatus);
                OrderHeader orderHeader = _db.OrderHeaders.Single(x => x.OrderHeaderId == orderId);
                if (newStatus == StaticData.OrderStatus.Cancelled.ToString() && orderHeader.PaymentIntentId != null)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.PaymentIntentId,                        
                    };
                    Log.Information("## Cancellation requested. Creating a refund in Stripe...");
                    var refundService = new RefundService();
                    Refund refund = refundService.Create(options);
                }
                orderHeader.Status = newStatus;
                _db.OrderHeaders.Update(orderHeader);
                await _db.SaveChangesAsync();
                Log.Information("## Order status updated.");
                _response.Result = orderHeader;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}", ex);
            }
            return _response;
        }

        [Authorize]
        [HttpPost("GetOrders")]
        public async Task<ResponseDto> GetOrders([FromBody]string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> orders;
                if(User.IsInRole(StaticData.Roles.ADMIN.ToString()))
                {
                    Log.Information("## Retrieving orders for ADMIN");
                    orders = _db.OrderHeaders.Include(x => x.OrderDetails).OrderByDescending(y => y.OrderHeaderId).ToList();
                }
                else
                {
                    Log.Information("## Retrieving orders for {userid}", userId);
                    orders = _db.OrderHeaders.Include(x => x.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(y => y.OrderHeaderId).ToList();
                }
                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orders);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}", ex);
            }
            return _response;
        }

        [Authorize]
        [HttpPost("GetOrder")]
        public async Task<ResponseDto> GetOrder([FromBody]int id)
        {
            try
            {
                Log.Information("## Retrieving order {Id}", id);
                OrderHeader orderHeader = _db.OrderHeaders.Include(x => x.OrderDetails).First(y => y.OrderHeaderId == id);
                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}", ex);
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
                    Log.Information("## Retrieving coupon - {CouponCode}", stripeRequestDto.OrderHeader.CouponCode);
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
                    else
                    {
                        Log.Warning("Failed to retrieve the coupon. {@Response}", response);
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
                Log.Information("## Creating stripe session...");
                Log.Debug("Stripe options: {@Options}", options);
                var service = new SessionService();
                Session session = service.Create(options);
                Log.Information("## Stripe session created. SessionID: {Id}", session.Id);

                stripeRequestDto.SessionUrl = session.Url;

                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();
                Log.Information("## Order {Id} updated.", orderHeader.OrderHeaderId);

                _response.Result = stripeRequestDto;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}" , ex);
            }
            return _response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody]int orderHeaderId)
        {
            try
            {
                Log.Information("## Retrieving order {id} ...", orderHeaderId);
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

                Log.Information("## Retrieving paymentIntent from Stripe. SessionId: {SessionId}", orderHeader.StripeSessionId);
                var service = new SessionService();
                Session session = service.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent != null && paymentIntent.Status == "succeeded")
                {
                    Log.Information("## Payment successful. Updating order status and paymentIntent : {PaymentIntentId} ...", paymentIntent.Id);
                    orderHeader.Status = StaticData.OrderStatus.Approved.ToString();
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    _db.Update(orderHeader);
                    await _db.SaveChangesAsync();

                    Log.Information("## Registering reward points ({RewardPoint}) in messaging queue...", Convert.ToInt32(orderHeader.OrderTotal));
                    RewardDto rewardDto = new()
                    {
                        OrderId = orderHeaderId,
                        RewardPoint = Convert.ToInt32(orderHeader.OrderTotal),
                        RewardDate = orderHeader.OrderDate,
                        UserId = orderHeader.UserId
                    };

                    string topicName = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedTopic");
                    await _messageBus.PublishMessageAsync(rewardDto, topicName);

                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }
                else
                {
                    Log.Warning("Payment unsuccessful. {@PaymentIntent}", paymentIntent);
                    _response.IsSuccess = false;
                    _response.Message = paymentIntent.Status;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Log.Error("Exception occured: {@ex}", ex);
            }
            return _response;
        }
    }
}

