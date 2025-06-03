using Bredinin.MyPetProject.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.MyPetProject.DAL;

public class ServiceContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceContext).Assembly);
    }
    
    public DbSet<User> Users { get; set; }
}