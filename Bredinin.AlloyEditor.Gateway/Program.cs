using Bredinin.AlloyEditor.Gateway.Core.Clients;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway Controllers", Version = "v1" });
});

builder.Services.AddClients(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();