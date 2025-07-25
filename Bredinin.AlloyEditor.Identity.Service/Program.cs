using Bredinin.AlloyEditor.Identity.Service;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Core.Http.Exceptions;
using Bredinin.AlloyEditor.Identity.Service.Core.Swagger;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler;
using Bredinin.AlloyEditor.Identity.Service.Migration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>

        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "identity-service",
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddAddAuthenticationCustom();
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
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCustomSwagger();
app.UseDatabaseMigrations();
app.UseAuthorization();

app.MapControllers();

app.Run();
