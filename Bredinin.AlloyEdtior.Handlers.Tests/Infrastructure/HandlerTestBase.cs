using Bredinin.AlloyEditor.Common.Configurations;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.Handlers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;

/// <summary>
/// База для тестов хендлеров: настоящая реляционная БД (SQLite in-memory),
/// чтобы работали ExecuteDelete, каскады и уникальные индексы, и DI-контейнер с AddHandlers().
/// </summary>
public abstract class HandlerTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    protected HandlerTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<ServiceDbContext>(o => o.UseSqlite(_connection));
        services.AddSingleton<IDistributedCache>(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        services.AddSingleton(Options.Create(new CacheSettings { ExpirationMinutes = 5 }));
        services.AddHandlers();

        _provider = services.BuildServiceProvider();
        Scope = _provider.CreateScope();

        Db = Scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        Db.Database.EnsureCreated();
    }

    protected IServiceScope Scope { get; }

    protected ServiceDbContext Db { get; }

    protected T Handler<T>() where T : notnull => Scope.ServiceProvider.GetRequiredService<T>();

    /// <summary>Контекст с чистым ChangeTracker — чтобы проверять то, что реально попало в БД</summary>
    protected ServiceDbContext FreshDb() =>
        new(new DbContextOptionsBuilder<ServiceDbContext>().UseSqlite(_connection).Options);

    protected DictChemicalElement AddElement(string symbol, bool isBase = false)
    {
        var element = new DictChemicalElement
        {
            Id = Guid.NewGuid(), Name = symbol, Symbol = symbol, IsBaseForAlloySystem = isBase
        };
        Db.DictChemicalElements.Add(element);
        Db.SaveChanges();
        return element;
    }

    protected AlloySystem AddSystem(DictChemicalElement baseElement, string name = "Iron")
    {
        var system = new AlloySystem { Id = Guid.NewGuid(), Name = name, BaseElementId = baseElement.Id };
        Db.AlloySystems.Add(system);
        Db.SaveChanges();
        return system;
    }

    protected AlloyGrade AddGrade(AlloySystem system, string name = "Steel", string? description = null)
    {
        var grade = new AlloyGrade
        {
            Id = Guid.NewGuid(), Name = name, Description = description, AlloySystemId = system.Id
        };
        Db.AlloyGrades.Add(grade);
        Db.SaveChanges();
        return grade;
    }

    protected DictTypeOfHeatTreatment AddHeatTreatmentType(string name = "Закалка")
    {
        var type = new DictTypeOfHeatTreatment
        {
            Id = Guid.NewGuid(), Name = name, Description = "desc", Code = name[..Math.Min(4, name.Length)]
        };
        Db.DictTypesOfHeatTreatments.Add(type);
        Db.SaveChanges();
        return type;
    }

    protected DictMechanicalPropertyType AddPropertyType(
        string name = "Предел прочности",
        PropertyValueType valueType = PropertyValueType.Range,
        decimal? minPossible = null,
        decimal? maxPossible = null)
    {
        var type = new DictMechanicalPropertyType
        {
            Id = Guid.NewGuid(), Name = name, Unit = "МПа", Symbol = "Rm", ValueType = valueType,
            MinPossible = minPossible, MaxPossible = maxPossible
        };
        Db.DictMechanicalPropertyTypes.Add(type);
        Db.SaveChanges();
        return type;
    }

    public void Dispose()
    {
        Scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
