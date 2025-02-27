using Bredinin.MyPetProject.DAL;
using Bredinin.MyPetProject.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


#region Services

builder.Services.AddDbContext<ServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();

#endregion


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ServiceContext>();
    context.Database.EnsureCreated(); 
}

app.UseHttpsRedirection();
app.UseCustomSwagger();
app.Run();
