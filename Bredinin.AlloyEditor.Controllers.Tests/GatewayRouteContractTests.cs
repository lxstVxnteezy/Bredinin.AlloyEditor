using System.Reflection;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Controllers;
using Bredinin.AlloyEditor.WebAPI.Controllers.Alloy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MvcHttpMethod = Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute;
using RefitHttpMethod = Refit.HttpMethodAttribute;

namespace Bredinin.AlloyEditor.Controllers.Tests;

/// <summary>
/// Gateway ходит в сервисы через Refit. Если маршрут или HTTP-метод клиента разъедется с контроллером сервиса,
/// запрос через gateway молча сломается — ловим это здесь.
/// </summary>
public class GatewayRouteContractTests
{
    private static readonly Type[] Clients =
        [typeof(IAlloyClient), typeof(IAlloySystemClient), typeof(IDictionaryClient), typeof(IAuthClient), typeof(IAdminClient)];

    private static string Normalize(string route) =>
        System.Text.RegularExpressions.Regex.Replace("/" + route.Trim('/'), @"\{[^}]+\}", "{}").ToLowerInvariant();

    private static IEnumerable<(string Verb, string Route)> ServiceEndpoints()
    {
        var controllers = typeof(AlloyController).Assembly.GetTypes()
            .Concat(typeof(AdminController).Assembly.GetTypes())
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var controller in controllers)
        {
            var prefix = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? "";
            foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var http in action.GetCustomAttributes<MvcHttpMethod>())
                {
                    var template = string.IsNullOrEmpty(http.Template) ? prefix : $"{prefix}/{http.Template}";
                    foreach (var verb in http.HttpMethods)
                        yield return (verb, Normalize(template));
                }
            }
        }
    }

    private static IEnumerable<(string Client, string Method, string Verb, string Route)> ClientCalls() =>
        from client in Clients
        from method in client.GetMethods()
        from refit in method.GetCustomAttributes<RefitHttpMethod>(true)
        select (client.Name, method.Name, refit.Method.Method, Normalize(refit.Path));

    [Fact]
    public void EveryRefitClientCall_ShouldMatchAnActionInTheDownstreamService()
    {
        var endpoints = ServiceEndpoints().ToHashSet();

        var calls = ClientCalls().ToArray();
        calls.Should().NotBeEmpty();
        foreach (var call in calls)
            endpoints.Should().Contain((call.Verb.ToUpperInvariant(), call.Route),
                $"{call.Client}.{call.Method} -> {call.Verb} {call.Route} должен существовать в сервисе");
    }
}
