﻿using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Bredinin.AlloyEditor.Identity.Service.Migration
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services,
            IConfiguration configuration)
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
                .WithGlobalConnectionString(connectionString)
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
                var commandBuilder = new NpgsqlCommandBuilder();

                var safeDatabaseName = commandBuilder.QuoteIdentifier(databaseName);

                using var cmdCreateDb = new NpgsqlCommand(
                    $"CREATE DATABASE {safeDatabaseName}",
                    connection);

                cmdCreateDb.ExecuteNonQuery();

            }
        }
    }
}
