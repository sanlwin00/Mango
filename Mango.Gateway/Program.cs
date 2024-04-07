using Mango.Gateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//* Add Ocelot for gateway
builder.Configuration.AddJsonFile("ocelot.json");
builder.Services.AddOcelot();               

//* Add Authentication, Authorization and Swagger
builder.AddJwtAuthenticationAndSwagger();   //*custom extension
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hellow World!");

app.UseOcelot().Wait();                //* use Ocelot

app.Run();
