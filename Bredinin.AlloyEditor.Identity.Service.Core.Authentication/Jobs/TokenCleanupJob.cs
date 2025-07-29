using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Jobs
{
    public class TokenCleanupJob(
        ILogger<TokenCleanupJob> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config) : BackgroundService
    {

        private readonly TimeSpan _interval = TimeSpan.FromHours(config.GetValue<int>("TokenCleanup:IntervalHours"));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Token cleanup job started at {UtcNow}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

                    await context.RefreshTokens
                        .Where(rt => rt.Expires < DateTime.UtcNow)
                        .ExecuteDeleteAsync(stoppingToken);
                
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during token cleanup");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
