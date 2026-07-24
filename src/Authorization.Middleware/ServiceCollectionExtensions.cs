using Authorization.Abstractions.Business;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Options;
using Authorization.Business;
using Authorization.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Middleware;

/// <summary>
/// Dependency-injection registration for the authorization stack. A single
/// call wires the connection factory, repository and business manager so
/// consumers no longer have to assemble the graph by hand.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything the claims-enrichment middleware needs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional hook to override connection-string name, claim type or stored-procedure names.</param>
    public static IServiceCollection AddAuthorizationClaims(
        this IServiceCollection services,
        Action<ClaimsEnrichmentOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<ClaimsEnrichmentOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        // The factory only caches an immutable connection string, so it is safe
        // as a singleton; it still hands out a fresh connection per call.
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ISecurityRepository, SecurityRepository>();
        services.AddScoped<IAuthorizationManager, AuthorizationManager>();

        return services;
    }
}
