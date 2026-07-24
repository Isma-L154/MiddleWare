using System.Security.Claims;
using Authorization.Abstractions.Business;
using Authorization.Abstractions.Models;
using Authorization.Abstractions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Middleware;

/// <summary>
/// Enriches the authenticated principal with claims resolved from the security
/// store: the user's identifier, name and email, plus a role claim per profile.
/// </summary>
/// <remarks>
/// Middleware is instantiated once (singleton), so no per-request state is kept
/// in fields — the scoped <see cref="IAuthorizationManager"/> is received through
/// <see cref="InvokeAsync"/> instead. Any failure while resolving identity data
/// is logged and swallowed so a transient store outage degrades gracefully
/// (the request continues unenriched) rather than crashing the pipeline.
/// </remarks>
public sealed partial class ClaimsEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsEnrichmentMiddleware> _logger;
    private readonly ClaimsEnrichmentOptions _options;

    public ClaimsEnrichmentMiddleware(
        RequestDelegate next,
        ILogger<ClaimsEnrichmentMiddleware> logger,
        IOptions<ClaimsEnrichmentOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    /// <summary>Intercepts the request, enriches the principal, then forwards it.</summary>
    public async Task InvokeAsync(HttpContext context, IAuthorizationManager authorizationManager)
    {
        var identity = await BuildClaimsIdentityAsync(context, authorizationManager);
        if (identity is not null)
        {
            context.User.AddIdentity(identity);
        }

        await _next(context);
    }

    private async Task<ClaimsIdentity?> BuildClaimsIdentityAsync(HttpContext context, IAuthorizationManager authorizationManager)
    {
        // Only enrich real, authenticated principals; anonymous traffic flows through untouched.
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        var userName = context.User.FindFirst(_options.UserNameClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(userName))
        {
            LogMissingUserNameClaim(_options.UserNameClaimType, context.TraceIdentifier);
            return null;
        }

        try
        {
            var claims = new List<Claim>();
            var user = await authorizationManager.GetUserAsync(new User { UserName = userName }, context.RequestAborted);
            if (user is null)
            {
                LogUserNotFound(context.TraceIdentifier);
                return null;
            }

            AddUserClaims(claims, user);
            await AddProfileClaimsAsync(claims, user, authorizationManager, context.RequestAborted);

            return new ClaimsIdentity(claims);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client went away; nothing to log at error level.
            return null;
        }
        catch (Exception ex)
        {
            // Graceful degradation: never let an identity-store failure crash the request.
            LogEnrichmentFailed(ex, context.TraceIdentifier);
            return null;
        }
    }

    private static void AddUserClaims(ICollection<Claim> claims, User user)
    {
        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrEmpty(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        }

        claims.Add(new Claim("IdUsuario", user.Id.ToString()));
    }

    private async Task AddProfileClaimsAsync(
        ICollection<Claim> claims,
        User user,
        IAuthorizationManager authorizationManager,
        CancellationToken cancellationToken)
    {
        var profiles = await authorizationManager.GetProfilesForUserAsync(user, cancellationToken);
        foreach (var profile in profiles)
        {
            claims.Add(new Claim(ClaimTypes.Role, profile.Id.ToString()));
        }
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Skipping claims enrichment: inbound principal has no '{ClaimType}' claim. TraceId={TraceId}")]
    private partial void LogMissingUserNameClaim(string claimType, string traceId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Skipping claims enrichment: no user matched the inbound claim. TraceId={TraceId}")]
    private partial void LogUserNotFound(string traceId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Claims enrichment failed; continuing without enriched claims. TraceId={TraceId}")]
    private partial void LogEnrichmentFailed(Exception exception, string traceId);
}
