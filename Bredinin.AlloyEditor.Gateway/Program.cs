using Bredinin.AlloyEditor.Common.Swagger;
using Bredinin.AlloyEditor.Gateway.Core.Authentication;
using Bredinin.AlloyEditor.Gateway.Core.Clients;
using Bredinin.AlloyEditor.Services.Common;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "Configs/jwtSettings.json"),
    optional: false,
    reloadOnChange: true);

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>

        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "api_gateway",
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

builder.Services.AddServiceSwagger(
    "Bredinin.ApiGateway", true);

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddSingleton<JwtProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHttpContextAccessor();


builder.Services.AddClients(builder.Configuration);

var app = builder.Build();


app.UseServiceSwaggerUi(uiTitle: "API Gateway");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();