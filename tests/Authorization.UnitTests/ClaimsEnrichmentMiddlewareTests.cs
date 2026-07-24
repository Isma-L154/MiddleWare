using System.Security.Claims;
using Authorization.Abstractions.Business;
using Authorization.Abstractions.Models;
using Authorization.Abstractions.Options;
using Authorization.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Authorization.UnitTests;

public class ClaimsEnrichmentMiddlewareTests
{
    private static readonly IOptions<ClaimsEnrichmentOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new ClaimsEnrichmentOptions());

    [Fact]
    public async Task Invoke_AnonymousUser_CallsNextWithoutEnrichment()
    {
        var manager = new Mock<IAuthorizationManager>(MockBehavior.Strict);
        var context = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new ClaimsEnrichmentMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, NullLogger<ClaimsEnrichmentMiddleware>.Instance, Options);

        await middleware.InvokeAsync(context, manager.Object);

        Assert.True(nextCalled);
        Assert.DoesNotContain(context.User.Identities, i => i.HasClaim(c => c.Type == ClaimTypes.Name));
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Invoke_AuthenticatedUser_AddsUserAndRoleClaims()
    {
        var userId = Guid.NewGuid();
        var manager = new Mock<IAuthorizationManager>();
        manager
            .Setup(m => m.GetUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, UserName = "jdoe", Email = "jdoe@example.com" });
        manager
            .Setup(m => m.GetProfilesForUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Profile { Id = 10, Name = "admin" }, new Profile { Id = 20, Name = "user" } });

        var context = BuildAuthenticatedContext("jdoe");
        var nextCalled = false;
        var middleware = new ClaimsEnrichmentMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, NullLogger<ClaimsEnrichmentMiddleware>.Instance, Options);

        await middleware.InvokeAsync(context, manager.Object);

        Assert.True(nextCalled);
        Assert.Equal("jdoe@example.com", context.User.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("jdoe", context.User.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(userId.ToString(), context.User.FindFirst("IdUsuario")?.Value);
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        Assert.Equal(new[] { "10", "20" }, roles);
    }

    [Fact]
    public async Task Invoke_MissingUserNameClaim_DoesNotEnrich()
    {
        var manager = new Mock<IAuthorizationManager>(MockBehavior.Strict);
        var context = BuildAuthenticatedContext(userName: null);
        var middleware = new ClaimsEnrichmentMiddleware(_ => Task.CompletedTask, NullLogger<ClaimsEnrichmentMiddleware>.Instance, Options);

        await middleware.InvokeAsync(context, manager.Object);

        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Invoke_UserNotFound_DoesNotAddClaims()
    {
        var manager = new Mock<IAuthorizationManager>();
        manager
            .Setup(m => m.GetUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var context = BuildAuthenticatedContext("ghost");
        var middleware = new ClaimsEnrichmentMiddleware(_ => Task.CompletedTask, NullLogger<ClaimsEnrichmentMiddleware>.Instance, Options);

        await middleware.InvokeAsync(context, manager.Object);

        Assert.Null(context.User.FindFirst("IdUsuario"));
        manager.Verify(m => m.GetProfilesForUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Invoke_StoreThrows_DegradesGracefullyAndCallsNext()
    {
        // The core resilience guarantee: an identity-store failure must never
        // crash the pipeline.
        var manager = new Mock<IAuthorizationManager>();
        manager
            .Setup(m => m.GetUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("database is down"));

        var context = BuildAuthenticatedContext("jdoe");
        var nextCalled = false;
        var middleware = new ClaimsEnrichmentMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, NullLogger<ClaimsEnrichmentMiddleware>.Instance, Options);

        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, manager.Object));

        Assert.Null(exception);
        Assert.True(nextCalled);
        Assert.Null(context.User.FindFirst("IdUsuario"));
    }

    private static DefaultHttpContext BuildAuthenticatedContext(string? userName)
    {
        var claims = new List<Claim>();
        if (userName is not null)
        {
            claims.Add(new Claim("usuario", userName));
        }

        // A non-null authentication type makes the identity report IsAuthenticated == true.
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }
}
