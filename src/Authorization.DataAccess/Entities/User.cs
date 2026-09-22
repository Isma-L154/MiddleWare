namespace Authorization.DataAccess.Entities;

// Row shape returned by the user stored procedure. Columns without a matching
// property (e.g. the password hash) are ignored by Dapper, so secrets are never
// materialized in memory.
internal sealed class User
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }
}
