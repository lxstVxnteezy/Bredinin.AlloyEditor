using Bredinin.AlloyEditor.Core.Authentication;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.Core.Metrics;
using Bredinin.AlloyEditor.Core.Swagger;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.DAL.Migration;
using Bredinin.AlloyEditor.Handlers;
using Bredinin.AlloyEditor.WebAPI;
using Prometheus;
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
builder.Services.AddServerMetrics();
builder.Services.AddDataAccess(builder.Configuration);

#endregion

var app = builder.Build();

#region Middlewares
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
app.UseHttpMetrics();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCustomSwagger();
app.UseDatabaseMigrations();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
    endpoints.MapControllers();
});


#endregion

app.Run();