namespace Authorization.Abstractions.Models;

/// <summary>
/// Domain model of a security profile (role) held by a user.
/// </summary>
public sealed class Profile
{
    /// <summary>Unique identifier of the profile.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the profile.</summary>
    public string? Name { get; set; }
}
