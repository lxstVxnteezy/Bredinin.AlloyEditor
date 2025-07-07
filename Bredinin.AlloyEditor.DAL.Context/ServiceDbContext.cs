using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.DAL;

public class ServiceDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
    }

    public DbSet<DictTypeOfHeatTreatment> DictTypesOfHeatTreatments { get; set; }
    public DbSet<AlloyGrade> AlloyGrades { get; set; }
    public DbSet<AlloyChemicalCompositions> AlloyChemicalCompositions { get; set; }
    public DbSet<DictChemicalElement> DictChemicalElements { get; set; }
    public DbSet<AlloySystem> AlloySystems { get; set; }
    public DbSet<AlloyHeatTreatment> AlloyHeatTreatments { get; set; }
}