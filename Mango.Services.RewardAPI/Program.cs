using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Services;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Extension;

var builder = WebApplication.CreateBuilder(args);


//* Add DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


//* Add singleton services
var dbOptionBuilder = new DbContextOptionsBuilder<AppDbContext>();
dbOptionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddSingleton(new RewardService(dbOptionBuilder.Options));
builder.Services.AddSingleton<IServiceBusCosumer, AzureServiceBusConsumer>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
ApplyMigration();           //* apply changes to db
app.UseServiceBusConsumer();//* extension to monitor service bus message queue
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

app.Run();
