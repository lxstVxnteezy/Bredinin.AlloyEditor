using Bredinin.MyPetProject.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.MyPetProject.DAL;

public class ServiceContext : DbContext
{ 
    public ServiceContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceContext).Assembly);
    }
    
    public DbSet<User> Users { get; set; }
}