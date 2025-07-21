using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.Core.Metrics;
using Bredinin.AlloyEditor.Core.Swagger;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.DAL.Migration;
using Bredinin.AlloyEditor.Handlers;
using Bredinin.AlloyEditor.WebAPI;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>

        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "alloyeditor_webapi",
                    autoGenerateServiceInstanceId: true))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter() 
            .AddProcessInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "otel-collector:4317");
            })
    );

builder.Services.AddSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddDatabaseMigrations(builder.Configuration);
builder.Services.AddHandlers();
builder.Services.AddServerMetrics();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); // Автопроверка для всех POST/PUT/PATCH/DELETE
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomSwagger();
app.UseDatabaseMigrations();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();