using Authorization.Abstractions.Business;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Options;
using Authorization.Business;
using Authorization.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Middleware;

/// <summary>
/// Dependency-injection registration for the authorization stack.
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

        // Validated at host start: the connection factory is resolved while
        // binding the middleware's scoped dependencies, outside its graceful
        // degradation, so a misconfiguration would otherwise fail every request.
        services.AddOptions<ClaimsEnrichmentOptions>()
            .Configure(options => configure?.Invoke(options))
            .Validate(HasRequiredSettings,
                "ClaimsEnrichmentOptions requires a connection string name, user name claim type and stored procedure names.")
            .Validate<IConfiguration>(
                (options, configuration) => !string.IsNullOrWhiteSpace(configuration.GetConnectionString(options.ConnectionStringName)),
                "The security database connection string was not found under \"ConnectionStrings\". Check ClaimsEnrichmentOptions.ConnectionStringName (default: SecurityDb).")
            .ValidateOnStart();

        // The factory only caches an immutable connection string, so it is safe
        // as a singleton; it still hands out a fresh connection per call.
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ISecurityRepository, SecurityRepository>();
        services.AddScoped<IAuthorizationManager, AuthorizationManager>();

        return services;
    }

    private static bool HasRequiredSettings(ClaimsEnrichmentOptions options) =>
        !string.IsNullOrWhiteSpace(options.ConnectionStringName) &&
        !string.IsNullOrWhiteSpace(options.UserNameClaimType) &&
        !string.IsNullOrWhiteSpace(options.GetUserProcedure) &&
        !string.IsNullOrWhiteSpace(options.GetProfilesProcedure);
}
