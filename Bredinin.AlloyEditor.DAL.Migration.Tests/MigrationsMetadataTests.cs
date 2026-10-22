using System.Reflection;
using Bredinin.AlloyEditor.DAL.Migration;
using FluentAssertions;
using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Tests;

public class MigrationsMetadataTests
{
    private static readonly Type[] MigrationTypes = typeof(DependenciesExtensions).Assembly
        .GetTypes()
        .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsAbstract)
        .ToArray();

    private static long VersionOf(Type type) => type.GetCustomAttribute<MigrationAttribute>()!.Version;

    [Fact]
    public void EveryMigration_ShouldHaveMigrationAttribute() =>
        MigrationTypes.Should().OnlyContain(t => t.GetCustomAttribute<MigrationAttribute>() != null);

    [Fact]
    public void MigrationVersions_ShouldBeUnique() =>
        MigrationTypes.Select(VersionOf).Should().OnlyHaveUniqueItems();

    [Fact]
    public void MigrationVersions_ShouldHave12Digits() =>
        MigrationTypes.Select(VersionOf).Should().OnlyContain(v => v.ToString().Length == 12);

    [Fact]
    public void ExecutionOrder_ShouldMatchChronologyInClassNames()
    {
        // Версии старых миграций записаны в формате yyyyddMM, а не yyyyMMdd, и уже применены в БД,
        // поэтому проверяем не формат, а то, что порядок выполнения совпадает с датой в имени класса.
        var dated = MigrationTypes
            .Select(t => (Type: t, Match: System.Text.RegularExpressions.Regex.Match(t.Name, @"_(\d{4})_(\d{2})_(\d{2})_(\d{4})$")))
            .Where(x => x.Match.Success)
            .ToArray();
        dated.Should().NotBeEmpty();

        var byVersion = dated.OrderBy(x => VersionOf(x.Type)).Select(x => x.Type.Name);
        var byName = dated.OrderBy(x => string.Concat(x.Match.Groups.Cast<System.Text.RegularExpressions.Group>().Skip(1).Select(g => g.Value)))
            .Select(x => x.Type.Name);

        byVersion.Should().Equal(byName);
    }

    [Fact]
    public void ShouldContainMigrationsForAllCoreTables() =>
        MigrationTypes.Select(t => t.Name).Should().Contain(n => n.Contains("InitTables"))
            .And.Contain(n => n.Contains("AlloyHeatTreatmentsTable"))
            .And.Contain(n => n.Contains("AlloyMechanicalPropertiesTable"));
}
