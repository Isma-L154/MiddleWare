namespace Authorization.Abstractions.Entities;

/// <summary>
/// Database-facing representation of a user, as returned by the security data store.
/// </summary>
public sealed class User
{
    /// <summary>Unique identifier of the user.</summary>
    public Guid Id { get; set; }

    /// <summary>Login name of the user.</summary>
    public string? UserName { get; set; }

    /// <summary>Hashed password. Never expose this outside the data layer.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Email address of the user.</summary>
    public string? Email { get; set; }
}
