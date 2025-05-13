using FluentMigrator.Runner;

namespace Bredinin.MyPetProject.WebAPI
{
    public static class DependenciesExtensions
    {
        public static IApplicationBuilder UseDatabaseMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();

            return app;
        }
    }
}
