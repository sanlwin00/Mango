using AutoMapper;
using Mango.Services.ProductAPI;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Extensions;
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
app.UseStaticFiles();       //* recognize wwwroot folder

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