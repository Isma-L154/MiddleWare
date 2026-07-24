using Authorization.Abstractions.Business;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Models;

namespace Authorization.Business;

/// <summary>
/// Default <see cref="IAuthorizationManager"/> that delegates identity
/// resolution to the security data-access layer. It is intentionally thin:
/// its role is to keep the middleware decoupled from persistence so business
/// rules can grow here without touching the pipeline.
/// </summary>
public sealed class AuthorizationManager : IAuthorizationManager
{
    private readonly ISecurityRepository _securityRepository;

    public AuthorizationManager(ISecurityRepository securityRepository)
    {
        _securityRepository = securityRepository ?? throw new ArgumentNullException(nameof(securityRepository));
    }

    /// <inheritdoc />
    public Task<User?> GetUserAsync(User user, CancellationToken cancellationToken = default)
        => _securityRepository.GetUserAsync(user, cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<Profile>> GetProfilesForUserAsync(User user, CancellationToken cancellationToken = default)
        => _securityRepository.GetProfilesForUserAsync(user, cancellationToken);
}
