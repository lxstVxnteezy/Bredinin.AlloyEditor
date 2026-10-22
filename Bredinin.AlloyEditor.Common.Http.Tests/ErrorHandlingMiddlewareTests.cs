using System.Net;
using System.Text.Json;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Common.Http.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bredinin.AlloyEditor.Common.Http.Tests;

public class ErrorHandlingMiddlewareTests
{
    private static async Task<(DefaultHttpContext Context, ErrorResponse? Body)> Run(RequestDelegate next)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await new ErrorHandlingMiddleware(next, NullLogger<ErrorHandlingMiddleware>.Instance).InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = context.Response.Body.Length == 0
            ? null
            : await JsonSerializer.DeserializeAsync<ErrorResponse>(context.Response.Body);
        return (context, body);
    }

    [Fact]
    public async Task WhenNoException_ShouldPassThroughUntouched()
    {
        var (context, body) = await Run(ctx => { ctx.Response.StatusCode = 204; return Task.CompletedTask; });

        context.Response.StatusCode.Should().Be(204);
        body.Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Bad request")]
    [InlineData(HttpStatusCode.NotFound, "Resource not found")]
    [InlineData(HttpStatusCode.Conflict, "Resource conflict")]
    [InlineData(HttpStatusCode.UnprocessableEntity, "Validation error")]
    [InlineData(HttpStatusCode.Forbidden, "An error occurred")]
    public async Task BusinessException_ShouldUseItsStatusCodeAndDetail(HttpStatusCode code, string message)
    {
        var (context, body) = await Run(_ => throw new BusinessException("что-то не так", code));

        context.Response.StatusCode.Should().Be((int)code);
        context.Response.ContentType.Should().Be("application/json");
        body.Should().BeEquivalentTo(new ErrorResponse((int)code, message, "что-то не так"));
    }

    [Fact]
    public async Task BusinessException_ByDefaultShouldBe400()
    {
        var (context, _) = await Run(_ => throw new BusinessException("x"));

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UnauthorizedAccessException_ShouldBe401NotServerError()
    {
        var (context, body) = await Run(_ => throw new UnauthorizedAccessException("Invalid credentials"));

        context.Response.StatusCode.Should().Be(401);
        body!.Message.Should().Be("Unauthorized");
        body.Detail.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task UnexpectedException_ShouldBe500()
    {
        var (context, body) = await Run(_ => throw new InvalidOperationException("boom"));

        context.Response.StatusCode.Should().Be(500);
        body!.StatusCode.Should().Be(500);
        body.Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public void BusinessException_ShouldKeepInnerException()
    {
        var inner = new Exception("inner");

        var exception = new BusinessException("outer", inner, HttpStatusCode.Conflict);

        exception.InnerException.Should().BeSameAs(inner);
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
