using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.WebAPI;
using Bredinin.MyPetProject.Core.Authentication;
using Bredinin.MyPetProject.DAL.Migration;
using Bredinin.MyPetProject.Handlers;
using Bredinin.MyPetProject.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddDatabaseMigrations(builder.Configuration);
builder.Services.AddAddAuthenticationCustom();
builder.Services.AddHandlers();
builder.Services.AddDataAccess(builder.Configuration);

#endregion

var app = builder.Build();

#region Middlewares

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCustomSwagger();
app.UseDatabaseMigrations();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
#endregion

app.Run();