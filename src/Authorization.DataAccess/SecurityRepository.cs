using System.Data;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Options;
using Authorization.Common;
using Dapper;
using Microsoft.Extensions.Options;
using Entities = Authorization.Abstractions.Entities;
using Models = Authorization.Abstractions.Models;

namespace Authorization.DataAccess;

/// <summary>
/// Reads users and their profiles from the security database through stored
/// procedures, using Dapper.
/// </summary>
public sealed class SecurityRepository : ISecurityRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ClaimsEnrichmentOptions _options;

    public SecurityRepository(IDbConnectionFactory connectionFactory, IOptions<ClaimsEnrichmentOptions> options)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    /// <inheritdoc />
    public async Task<Models.User?> GetUserAsync(Models.User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        // A fresh connection per call: SqlConnection is not thread-safe and
        // must not be shared across concurrent requests. `await using` guarantees
        // it is returned to the pool even if the query throws.
        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            _options.GetUserProcedure,
            new { user.Email, user.UserName },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var entity = await connection.QueryFirstOrDefaultAsync<Entities.User>(command);
        return Converter.Convert<Entities.User, Models.User>(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Models.Profile>> GetProfilesForUserAsync(Models.User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            _options.GetProfilesProcedure,
            new { user.Email, user.UserName },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var entities = await connection.QueryAsync<Entities.Profile>(command);
        return Converter.ConvertList<Entities.Profile, Models.Profile>(entities);
    }
}
