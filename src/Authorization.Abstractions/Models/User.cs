namespace Authorization.Abstractions.Models;

/// <summary>
/// Domain model of a user, used by the business and middleware layers.
/// Kept separate from the database entity so persistence changes never leak
/// into the pipeline contract.
/// </summary>
public sealed class User
{
    /// <summary>Unique identifier of the user.</summary>
    public Guid Id { get; set; }

    /// <summary>Login name of the user.</summary>
    public string? UserName { get; set; }

    /// <summary>Hashed password. Never surfaced in claims, logs or responses.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Email address of the user.</summary>
    public string? Email { get; set; }
}
