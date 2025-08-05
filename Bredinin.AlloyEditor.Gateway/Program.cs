using Bredinin.AlloyEditor.Gateway.Core.Authentication;
using Bredinin.AlloyEditor.Gateway.Core.Clients;
using Bredinin.AlloyEditor.Gateway.Core.Swagger;
using Bredinin.AlloyEditor.Services.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "Configs/jwtSettings.json"),
    optional: false,
    reloadOnChange: true);

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddSingleton<JwtProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddAddAuthenticationCustom(builder.Configuration);
builder.Services.AddHttpContextAccessor();


builder.Services.AddClients(builder.Configuration);

var app = builder.Build();

app.UseCustomSwagger();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();