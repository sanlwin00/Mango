using Mango.Web;
using Mango.Web.Middleware;
using Mango.Web.Services;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); 
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddApplicationInsightsTelemetry();  //* Azure App Insights
builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add<CartItemCountActionFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<ICouponService, CouponService>();
builder.Services.AddHttpClient<IProductService, ProductService>();
builder.Services.AddHttpClient<IShoppingCartService, ShoppingCartService>();

StaticData.AuthApiBaseUrl = builder.Configuration["ServiceUrls:AuthAPI"];
StaticData.CouponApiBaseUrl = builder.Configuration["ServiceUrls:CouponAPI"];
StaticData.ProductApiBaseUrl = builder.Configuration["ServiceUrls:ProductAPI"];
StaticData.ShoppingCartApiBaseUrl = builder.Configuration["ServiceUrls:ShoppingCartAPI"];
StaticData.OrderApiBaseUrl = builder.Configuration["ServiceUrls:OrderAPI"];

builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie( option =>
    {
        option.ExpireTimeSpan = TimeSpan.FromHours(8);
        option.LoginPath = "/auth/login";
        option.AccessDeniedPath = "/auth/accessdenied";
    });

//* Configure SeriLog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();
app.UseMiddleware<RequestLogContextEnricher>();
app.UseSerilogRequestLogging(); //* log every request

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
