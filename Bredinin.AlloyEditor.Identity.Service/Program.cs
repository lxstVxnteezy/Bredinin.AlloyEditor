using Bredinin.AlloyEditor.Identity.Service;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Core.Http.Exceptions;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler;
using Bredinin.AlloyEditor.Identity.Service.Migration;
using Bredinin.AlloyEditor.Services.Common;
using Bredinin.AlloyEditor.Common.Swagger;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "Configs/jwtSettings.json"),
    optional: false,
    reloadOnChange: true);

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddSingleton<JwtProvider>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>

        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "identity-service",
                    autoGenerateServiceInstanceId: true))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "otel-collector:4317");
            })
    );
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    options.Configuration = redisConnectionString;
    options.InstanceName = "auth_:";
});


builder.Services.AddSerilog();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAddAuthenticationCustom(builder.Configuration);
builder.Services.AddServiceSwagger("Bredinin.Identity.Service",true);

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddDatabaseMigrations(builder.Configuration);
builder.Services.AddApplicationHandlers();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseServiceSwaggerUi(uiTitle: "Identity Service API");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseDatabaseMigrations();
app.UseAuthorization();

app.MapControllers();

app.Run();
