using Bredinin.AlloyEditor.Handlers;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEdtior.Handlers.Tests;

public class HandlersRegistrationTests
{
    [Fact]
    public void AddHandlers_ShouldRegisterEveryHandlerInterface()
    {
        var services = new ServiceCollection().AddHandlers();

        var handlerInterfaces = typeof(IHandler).Assembly.GetTypes()
            .Where(t => t.IsInterface && t != typeof(IHandler) && typeof(IHandler).IsAssignableFrom(t))
            .ToArray();

        handlerInterfaces.Should().NotBeEmpty();
        foreach (var handlerInterface in handlerInterfaces)
            services.Should().Contain(d => d.ServiceType == handlerInterface, because: handlerInterface.Name);
    }

    [Fact]
    public void AddHandlers_ShouldRegisterHandlersAsTransient()
    {
        var services = new ServiceCollection().AddHandlers();

        services.Single(d => d.ServiceType == typeof(ICreateAlloyGradeHandler))
            .Lifetime.Should().Be(ServiceLifetime.Transient);
    }
}
