using System.Data.Common;

namespace Authorization.Abstractions.DataAccess;

/// <summary>
/// Creates database connections on demand.
/// </summary>
/// <remarks>
/// Every call returns a brand-new, unopened connection. This is deliberate:
/// a single <see cref="DbConnection"/> is not thread-safe, so sharing one
/// instance across concurrent requests corrupts state. Handing out fresh
/// connections lets the underlying ADO.NET connection pool do its job.
/// The abstract <see cref="DbConnection"/> return type keeps this contract
/// free of any concrete database-provider dependency.
/// </remarks>
public interface IDbConnectionFactory
{
    /// <summary>Creates a new, unopened database connection.</summary>
    DbConnection CreateConnection();
}
