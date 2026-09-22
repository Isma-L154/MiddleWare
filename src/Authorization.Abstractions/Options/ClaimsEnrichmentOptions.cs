namespace Authorization.Abstractions.Options;

/// <summary>
/// Configuration for the claims-enrichment middleware and the security store.
/// </summary>
public sealed class ClaimsEnrichmentOptions
{
    /// <summary>
    /// Name of the connection string (in <c>ConnectionStrings</c>) pointing at
    /// the security database. Defaults to <c>SecurityDb</c>.
    /// </summary>
    public string ConnectionStringName { get; set; } = "SecurityDb";

    /// <summary>
    /// The inbound JWT claim type that carries the user name used to look the
    /// user up. Defaults to <c>usuario</c>.
    /// </summary>
    public string UserNameClaimType { get; set; } = "usuario";

    /// <summary>
    /// Stored procedure that returns a single user by user name / email.
    /// Defaults to <c>ObtenerUsuario</c>.
    /// </summary>
    public string GetUserProcedure { get; set; } = "ObtenerUsuario";

    /// <summary>
    /// Stored procedure that returns the profiles for a user.
    /// Defaults to <c>ObtenerPerfilesxUsuario</c>.
    /// </summary>
    public string GetProfilesProcedure { get; set; } = "ObtenerPerfilesxUsuario";
}
