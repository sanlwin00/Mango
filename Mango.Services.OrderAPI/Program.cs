using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Extensions;
using Mango.Services.OrderAPI.Services;
using Mango.Services.OrderAPI.Services.IServices;
using Mango.Services.OrderAPI.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();  //* Azure App Insights
//* Add DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)))    //* suppress EF SQL commands in the log
            .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)))
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});
 
//* Add Automapper
IMapper mapper = MappingConfig.RegisterMappings().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//* Add Middleware to intercept outgoing backend API calls and inject Bearer token
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();

//* Add Http and dependency services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddSingleton<IMessageBus>(new AzureMessageBus(builder.Configuration["Azure:ServiceBusConnectionString"]));
builder.Services.AddHttpClient("Product"
    , x => x.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"]))
    .AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddHttpClient("Coupon"
    , x => x.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponAPI"]))
    .AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//* Add Authentication, Authorization and Swagger
builder.AddJwtAuthenticationAndSwagger(); //custom extension
builder.Services.AddAuthorization();

//* Configure SeriLog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API");
        options.RoutePrefix = string.Empty;
    });
}

// This is for Stripe API
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.UseSerilogRequestLogging(); //* log every request

app.UseHttpsRedirection();
app.UseAuthentication();    //* use jwt auth
app.UseAuthorization();

app.MapControllers();
ApplyMigration();           //* apply changes to db
app.Run();

//* Auto apply any pending migration
void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}