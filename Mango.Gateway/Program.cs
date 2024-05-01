using Mango.Gateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();  //* Azure App Insights
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//* Add Ocelot for gateway
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("ocelot.Production.json", optional: false, reloadOnChange: true);
    Log.Information("Loaded ocelot.Production.json");
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
    Log.Information("Loaded ocelot.json");
}
builder.Services.AddOcelot();               

//* Add Authentication, Authorization and Swagger
builder.AddJwtAuthenticationAndSwagger();   //*custom extension
builder.Services.AddAuthorization();
//* Add Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();
var app = builder.Build();
app.UseSerilogRequestLogging(); //* log every request
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.UseOcelot().Wait();                //* use Ocelot

app.Run();
