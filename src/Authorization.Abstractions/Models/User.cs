namespace Authorization.Abstractions.Models;

/// <summary>
/// Domain model of a user, used by the business and middleware layers.
/// </summary>
public sealed class User
{
    /// <summary>Unique identifier of the user.</summary>
    public Guid Id { get; set; }

    /// <summary>Login name of the user.</summary>
    public string? UserName { get; set; }

    /// <summary>Email address of the user.</summary>
    public string? Email { get; set; }
}
