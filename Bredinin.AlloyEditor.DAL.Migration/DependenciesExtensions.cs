using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Bredinin.MyPetProject.DAL.Migration;

public static class DependenciesExtensions
{
    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => ConfigureRunner(rb, configuration))
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    private static void ConfigureRunner(IMigrationRunnerBuilder runnerBuilder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (connectionString == null)
            throw new Exception("Not found connectionString ");

        EnsureDatabaseExists(connectionString);

        runnerBuilder
            .AddPostgres()
            .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
            .ScanIn(typeof(DependenciesExtensions).Assembly).For.Migrations();
    }

    private static void EnsureDatabaseExists(string connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;

        connectionStringBuilder.Database = "postgres";

        using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
        connection.Open();

        using var cmdCheckDb = new NpgsqlCommand(
            $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", connection);
        var exists = cmdCheckDb.ExecuteScalar() != null;

        if (!exists)
        {
            using var cmdCreateDb = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName}\"", connection);
            cmdCreateDb.ExecuteNonQuery();
        }
    }

}