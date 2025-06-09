using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.WebAPI;
using Bredinin.MyPetProject.Core.Authentication;
using Bredinin.MyPetProject.DAL.Migration;
using Bredinin.MyPetProject.Handlers;
using Bredinin.MyPetProject.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

#region Services

builder.Services.AddSerilog();
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
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
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