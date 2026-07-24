using Authorization.Abstractions.Models;

namespace Authorization.Abstractions.DataAccess;

/// <summary>
/// Data-access contract for the security store that backs authorization.
/// </summary>
public interface ISecurityRepository
{
    /// <summary>Reads a single user matching the supplied lookup criteria.</summary>
    Task<User?> GetUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Reads every profile (role) granted to the supplied user.</summary>
    Task<IEnumerable<Profile>> GetProfilesForUserAsync(User user, CancellationToken cancellationToken = default);
}
