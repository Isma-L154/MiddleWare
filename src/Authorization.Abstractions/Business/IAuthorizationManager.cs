using Authorization.Abstractions.Models;

namespace Authorization.Abstractions.Business;

/// <summary>
/// Business-layer entry point for resolving the identity data used to enrich
/// the request principal (the current user and the profiles they hold).
/// </summary>
public interface IAuthorizationManager
{
    /// <summary>
    /// Resolves the full user record for the supplied lookup criteria.
    /// </summary>
    /// <param name="user">Partial user carrying at least a lookup key (user name or email).</param>
    /// <param name="cancellationToken">Token used to cancel the underlying I/O.</param>
    /// <returns>The resolved user, or <c>null</c> when no match exists.</returns>
    Task<User?> GetUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves every profile (role) granted to the supplied user.
    /// </summary>
    /// <param name="user">User whose profiles are requested.</param>
    /// <param name="cancellationToken">Token used to cancel the underlying I/O.</param>
    /// <returns>The profiles for the user; an empty sequence when none exist.</returns>
    Task<IEnumerable<Profile>> GetProfilesForUserAsync(User user, CancellationToken cancellationToken = default);
}
