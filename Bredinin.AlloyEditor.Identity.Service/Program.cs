using Bredinin.AlloyEditor.Identity.Service;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Core.Swagger;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler;
using Bredinin.AlloyEditor.Identity.Service.Migration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddAddAuthenticationCustom();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddDatabaseMigrations(builder.Configuration);
builder.Services.AddApplicationHandlers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomSwagger();
app.UseDatabaseMigrations();
app.UseAuthorization();

app.MapControllers();

app.Run();
