using Microsoft.AspNetCore.Builder;

namespace Authorization.Middleware;

/// <summary>
/// Pipeline registration for <see cref="ClaimsEnrichmentMiddleware"/>.
/// </summary>
public static class AuthorizationMiddlewareExtensions
{
    /// <summary>
    /// Adds the claims-enrichment middleware to the request pipeline. Place it
    /// after authentication so the principal is already established.
    /// </summary>
    public static IApplicationBuilder UseAuthorizationClaims(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.UseMiddleware<ClaimsEnrichmentMiddleware>();
    }
}
