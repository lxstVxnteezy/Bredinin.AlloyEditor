using System.Text.Json;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Entities;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Identity.Service.Handler.Identity;
using Bredinin.AlloyEditor.Services.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bredinin.AlloyEditor.Identity.Tests;

public class AuthTests
{
    [Fact]
    public async Task RotateAsync_ShouldSaveNewValue_BeforeRemovingOldValue()
    {
        // Arrange
        const string oldRefreshToken = "old-refresh-token";
        const string newRefreshToken = "new-refresh-token";
        
        var UserId = Guid.NewGuid();
        var dbOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new IdentityDbContext(dbOptions);
        var tokenServiceMock = new Mock<ITokenService>();
       
        context.Users.Add(new User
        {
            Id = UserId,
            Login = "Test",
            UserRoles = new List<UserRole>(),
            FirstName = "Test",
            LastName = "Test",
            SecondName = "Test",
            Hash = "Test"
        });

        await context.SaveChangesAsync();
        
        var jwtOptions = Options.Create(new JwtConfiguration
        {
            Key = "test-key-at-least-32-characters-long!!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        });
        var jwtOptionsAccessor = new JwtOptionsAccessor(jwtOptions);

        tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
        tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(newRefreshToken);
        tokenServiceMock
            .Setup(t => t.CreateAuthResponse(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new AuthResponse("new-access-token", newRefreshToken));

        var cacheMock = new Mock<IDistributedCache>();
        var sequence = new MockSequence();
 
        var existingEntry = new RefreshTokenCacheEntry
        {
            UserId = UserId,
            Expires = DateTime.UtcNow.AddDays(1)
        };
        cacheMock
            .Setup(c => c.GetAsync("refresh_" + oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.SerializeToUtf8Bytes(existingEntry));
 
        cacheMock.InSequence(sequence)
            .Setup(c => c.SetAsync(
                "refresh_" + newRefreshToken,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        cacheMock.InSequence(sequence)
            .Setup(c => c.RemoveAsync("refresh_" + oldRefreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        var handler = new RefreshTokenQueryHandler(
            tokenServiceMock.Object,
            context,
            cacheMock.Object,
            jwtOptionsAccessor);
 
        // Act
        var result = await handler.Handle(new RefreshTokenQuery(oldRefreshToken), CancellationToken.None);
 
        // Assert
        result.Should().NotBeNull();
        result.RefreshToken.Should().Be(newRefreshToken);
 
        cacheMock.Verify(c => c.SetAsync(
                "refresh_" + newRefreshToken, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        cacheMock.Verify(c => c.RemoveAsync("refresh_" + oldRefreshToken, It.IsAny<CancellationToken>()), Times.Once);



    }
}

