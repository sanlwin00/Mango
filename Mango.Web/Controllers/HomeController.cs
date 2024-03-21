using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Newtonsoft.Json;
using Mango.Web.Services.IServices;
using Mango.Web.Services;
using Mango.Web.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCart;

    public HomeController(ILogger<HomeController> logger, IProductService productService, IShoppingCartService shoppingCart)
    {
        _logger = logger;
        _productService = productService;
        _shoppingCart = shoppingCart;
    }

    public async Task<IActionResult> Index()
    {
        List<ProductDto>? list = new();

        ResponseDto? response = await _productService.GetAllProductAsync();
        if (response != null && response.IsSuccess)
        {
            list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            if(User.Identity.IsAuthenticated)
            {
                var userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
                if (userId != null)
                {
                    ResponseDto? responseDto = await _shoppingCart.GetCartByUserAsync(userId);
                    if (responseDto != null && responseDto.IsSuccess)
                    {
                        CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(responseDto.Result)); 
                        if (cartDto != null)
                        {
                            TempData["cartItemCount"] = cartDto.CartDetails.Count();
                        }
                    }
                }
            }
            
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return View(list);

    }

    public async Task<IActionResult> ProductDetails(int productId)
    {
        ProductDto? dto = new();

        ResponseDto? response = await _productService.GetProductbyIdAsync(productId);
        if (response != null && response.IsSuccess)
        {
            dto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return View(dto);

    }
    
    [Authorize]
    [HttpPost("AddToCart")]
    public async Task<IActionResult> AddToCart(ProductDto product)
    {
        var userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
        CartHeaderDto cartHeader = new(){ UserId = userId };
        List<CartDetailDto> cartDetails = new();
        cartDetails.Add(new CartDetailDto { ProductId = product.ProductId, Qty = product.Qty});

        CartDto cart = new(){ CartHeader = cartHeader, CartDetails = cartDetails};        
        string temp = JsonConvert.SerializeObject(cart);
        ResponseDto? response = await _shoppingCart.UpsertCartAsync(cart);
        if (response != null && response.IsSuccess)
        {
            cart = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
            return RedirectToAction("CartIndex","Cart");
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return RedirectToAction("ProductDetails", new { productId = product.ProductId });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
