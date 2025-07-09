using Bredinin.AlloyEditor.Gateway.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ������������
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ocelot � Swagger
builder.Services.AddGatewayOcelot(builder.Configuration);

// JWT ��������������
builder.Services.AddJwtAuthentication(builder.Configuration);

// �����������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway Controllers", Version = "v1" });
});

var app = builder.Build();

// Middleware pipeline
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseGatewayOcelot();
app.MapControllers();

app.Run();