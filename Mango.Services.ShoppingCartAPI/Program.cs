using AutoMapper;
using Mango.Services.ShoppingCartAPI;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Extensions;
using Mango.Services.ShoppingCartAPI.Services;
using Mango.Services.ShoppingCartAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//* Add DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//* Add Automapper
IMapper mapper = MappingConfig.RegisterMappings().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//* Add Http and dependency services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddHttpClient("Product", x => x.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"]));
builder.Services.AddHttpClient("Coupon", x => x.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponAPI"]));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//* Add Authentication, Authorization and Swagger
builder.AddJwtAuthenticationAndSwagger(); //custom extension
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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