using Bredinin.MyPetProject.Core.Authentication;
using Bredinin.MyPetProject.DAL;
using Bredinin.MyPetProject.DAL.Migration;
using Bredinin.MyPetProject.Handlers;
using Bredinin.MyPetProject.Swagger;
using Bredinin.MyPetProject.WebAPI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddDbContext<ServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddDatabaseMigrations(builder.Configuration);
builder.Services.AddAddAuthenticationCustom();
builder.Services.AddHandlersScopedServices();

#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ServiceContext>();
}

#region Middlewares

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCustomSwagger();
app.UseDatabaseMigrations();

#endregion

app.Run();