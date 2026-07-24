using System.Data.Common;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Authorization.DataAccess;

/// <summary>
/// SQL Server implementation of <see cref="IDbConnectionFactory"/>.
/// </summary>
/// <remarks>
/// The connection string is read once and cached; each call returns a fresh
/// <see cref="SqlConnection"/> so callers can safely open, use and dispose it
/// without any cross-request sharing. ADO.NET pools the physical connections
/// underneath, so this is both correct and cheap.
/// </remarks>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration, IOptions<ClaimsEnrichmentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);

        var name = options.Value.ConnectionStringName;
        _connectionString = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException(
                $"Connection string '{name}' was not found. Configure it under \"ConnectionStrings\".");
    }

    /// <inheritdoc />
    public DbConnection CreateConnection() => new SqlConnection(_connectionString);
}
