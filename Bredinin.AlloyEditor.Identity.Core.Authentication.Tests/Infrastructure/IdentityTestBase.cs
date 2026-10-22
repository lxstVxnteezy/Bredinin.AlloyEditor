using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Identity.Service.Handler;
using Bredinin.AlloyEditor.Services.Common;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Role = Bredinin.AlloyEditor.Identity.Service.Domain.Role;

namespace Bredinin.AlloyEditor.Identity.Tests.Infrastructure;

/// <summary>
/// Настоящие хендлеры через MediatR + SQLite in-memory + кэш в памяти + поддельный Redis (множества в словаре).
/// </summary>
public abstract class IdentityTestBase : IDisposable
{
    public const string TestPassword = "Passw0rdX";

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    protected IdentityTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        Jwt = new JwtConfiguration
        {
            Key = "test-key-at-least-32-characters-long!!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        };

        RedisSets = new Dictionary<string, HashSet<string>>();
        var redisDb = new Mock<IDatabase>();
        redisDb.Setup(d => d.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisKey key, RedisValue value, CommandFlags _) =>
            {
                var set = RedisSets.TryGetValue(key.ToString(), out var existing) ? existing : RedisSets[key.ToString()] = [];
                return set.Add(value.ToString());
            });
        redisDb.Setup(d => d.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisKey key, CommandFlags _) =>
                RedisSets.TryGetValue(key.ToString(), out var set)
                    ? set.Select(x => (RedisValue)x).ToArray()
                    : []);
        redisDb.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisKey key, CommandFlags _) => RedisSets.Remove(key.ToString()));
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(redisDb.Object);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<IdentityDbContext>(o => o.UseSqlite(_connection));
        services.AddSingleton<IDistributedCache>(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        services.AddSingleton(redis.Object);
        services.AddSingleton(new JwtOptionsAccessor(Options.Create(Jwt)));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddApplicationHandlers();

        _provider = services.BuildServiceProvider();
        Scope = _provider.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        Db.Database.EnsureCreated();
    }

    protected JwtConfiguration Jwt { get; }

    protected Dictionary<string, HashSet<string>> RedisSets { get; }

    protected IServiceScope Scope { get; }

    protected IdentityDbContext Db { get; }

    protected IMediator Mediator => Scope.ServiceProvider.GetRequiredService<IMediator>();

    protected IDistributedCache Cache => Scope.ServiceProvider.GetRequiredService<IDistributedCache>();

    protected IdentityDbContext FreshDb() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>().UseSqlite(_connection).Options);

    protected Role AddRole(string name)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = name, Description = name };
        Db.Roles.Add(role);
        Db.SaveChanges();
        return role;
    }

    protected User AddUser(string login = "ivan", string password = TestPassword, params Role[] roles)
    {
        var user = new User
        {
            Id = Guid.NewGuid(), Login = login, FirstName = "Иван", LastName = "Петров", SecondName = "С", Age = 30,
            Hash = new PasswordHasher().Generate(password)
        };
        foreach (var role in roles)
            user.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleId = role.Id });
        Db.Users.Add(user);
        Db.SaveChanges();
        return user;
    }

    public void Dispose()
    {
        Scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
