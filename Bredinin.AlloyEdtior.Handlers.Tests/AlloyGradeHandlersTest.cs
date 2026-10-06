using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests;

public class AlloyGradeHandlersTest
{
    private readonly SqliteConnection _connection;
    private readonly ServiceDbContext _context;

    #region TestsEntities

    private readonly DictChemicalElement _chemicalElement;
    private readonly AlloySystem _alloySystem;


    #endregion

    public AlloyGradeHandlersTest()
    {
        // Открываем SQLite-соединение в памяти. Важно: соединение нужно
        // держать открытым всё время жизни теста — если закроется,
        // in-memory база SQLite исчезает полностью.
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var dbOptions = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ServiceDbContext(dbOptions);
        _context.Database.EnsureCreated(); // создаёт таблицы по твоим EF-моделям

        _chemicalElement = new DictChemicalElement
        {
            Id = Guid.NewGuid(),
            AtomicNumber = 26,
            Name = "Iron",
            Symbol = "Fe",
            AtomicWeight = 56,
            Group = 8,
            Period = 4,
            Density = 8,
            Description = "Основной элемент сталей и чугунов",
            IsBaseForAlloySystem = true
        };

        _alloySystem = new AlloySystem
        {
            Id = Guid.NewGuid(),
            Name = "Сталь",
            BaseElementId = _chemicalElement.Id,
            BaseElement = _chemicalElement,
            Description = "alloy"
        };

        _context.DictChemicalElements.Add(_chemicalElement);
        _context.AlloySystems.Add(_alloySystem);

        _context.SaveChanges();
    }

    [Fact]
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose(); // обязательно закрыть — иначе соединения будут утекать между тестами
    }

    [Fact]
    public async Task CreateAlloyGradeHandlerTest()
    {
        //Arange
        var request = new CreateAlloyGradeRequest(
            Name: "Ст.45",
            Description: "123",
            AlloySystemId: _alloySystem.Id,
            ChemicalCompositions:
            [
                new CreateChemicalCompositionRequest(
                    MinValue: 80,
                    MaxValue: 85,
                    ExactValue: null,
                    ChemicalElementId: _chemicalElement.Id)
            ],
            HeatTreatments: null,
            DefaultMechanicalProperties: null
        );

        var handler = new CreateAlloyGradeHandler(_context);

        //Act
        var result = await handler.Handle(request, CancellationToken.None);


        //Assert 
        _context.AlloyGrades.Count().Should().Be(1);
        _context.AlloyGrades.Any(x => x.Id == result).Should().BeTrue();

        var savedGrade = await _context.AlloyGrades
            .Include(x => x.ChemicalCompositions)
            .FirstOrDefaultAsync(x => x.Id == result);

        savedGrade.Should().NotBeNull();
        savedGrade!.Name.Should().Be("Ст.45");
        savedGrade.Description.Should().Be("123");
        savedGrade.AlloySystemId.Should().Be(_alloySystem.Id);

        savedGrade.ChemicalCompositions.Should().HaveCount(1);
        savedGrade.ChemicalCompositions.First().MinValue.Should().Be(80);
        savedGrade.ChemicalCompositions.First().MaxValue.Should().Be(85);
    }

    [Fact]
    public async Task CreateAlloyGradeHandlerTestWhenAlloySystemNotFoundShouldThrow()
    {
        //Arange
        var request = new CreateAlloyGradeRequest(
            Name: "Ст.45",
            Description: "123",
            AlloySystemId: Guid.NewGuid(),
            ChemicalCompositions:
            [
                new CreateChemicalCompositionRequest(
                    MinValue: 80,
                    MaxValue: 85,
                    ExactValue: null,
                    ChemicalElementId: _chemicalElement.Id)
            ],
            HeatTreatments: null,
            DefaultMechanicalProperties: null
        );

        var handler = new CreateAlloyGradeHandler(_context);

        //Act
        var act = () => handler.Handle(request, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task DeleteAlloyGradeHandlerTest()
    {
        //Arange
        var alloyGrade = new AlloyGrade
        {
            Id = Guid.NewGuid(),
            Name = "Ст.45",
            AlloySystemId = _alloySystem.Id,
            AlloySystem = _alloySystem,
            ChemicalCompositions = new List<AlloyChemicalCompositions>{new AlloyChemicalCompositions
            {
                Id = Guid.NewGuid(),
                MinValue = 80,
                MaxValue =  85,
                ExactValue = null,
                ChemicalElementId =  _chemicalElement.Id,
                ChemicalElement =  _chemicalElement,
            }}
        };

        _context.AlloyGrades.Add(alloyGrade);
        await _context.SaveChangesAsync();

        var handler = new DeleteAlloyGradeHandler(_context);

        //Act
        await handler.Handle(alloyGrade.Id, CancellationToken.None);

        //Assert 
        _context.AlloyGrades.Should().BeEmpty();
        _context.AlloyChemicalCompositions.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAlloyGradeHandlerTestNotFoundGrades()
    {
        //Arange
        var alloyGrade = new AlloyGrade
        {
            Id = Guid.NewGuid(),
            Name = "Ст.45",
            Description = "123",
            AlloySystemId = _alloySystem.Id,
            AlloySystem = _alloySystem,
            ChemicalCompositions = new List<AlloyChemicalCompositions>{new AlloyChemicalCompositions
            {
                Id = Guid.NewGuid(),
                MinValue = 80,
                MaxValue =  85,
                ExactValue = null,
                ChemicalElementId =  _chemicalElement.Id,
                ChemicalElement =  _chemicalElement,
            }}
        };

        _context.AlloyGrades.Add(alloyGrade);
        await _context.SaveChangesAsync();

        var handler = new DeleteAlloyGradeHandler(_context);

        //Act
        var act = () => handler.Handle(Guid.NewGuid(), CancellationToken.None);

        //Assert 
        _context.AlloyGrades.Should().HaveCount(1);
        _context.AlloyChemicalCompositions.Should().HaveCount(1);

        var savedGrade = await _context.AlloyGrades
            .Include(x => x.ChemicalCompositions)
            .FirstOrDefaultAsync();

        savedGrade.Should().NotBeNull();
        savedGrade!.Name.Should().Be("Ст.45");
        savedGrade.Description.Should().Be("123");
        savedGrade.AlloySystemId.Should().Be(_alloySystem.Id);

        savedGrade.ChemicalCompositions.Should().HaveCount(1);
        savedGrade.ChemicalCompositions.First().MinValue.Should().Be(80);
        savedGrade.ChemicalCompositions.First().MaxValue.Should().Be(85);
    }

}
