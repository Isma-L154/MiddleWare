namespace Authorization.Abstractions.Entities;

/// <summary>
/// Database-facing representation of a security profile (role) a user can hold.
/// </summary>
public sealed class Profile
{
    /// <summary>Unique identifier of the profile.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the profile.</summary>
    public string? Name { get; set; }
}
