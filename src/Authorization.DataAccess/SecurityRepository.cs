using System.Data;
using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Options;
using Dapper;
using Microsoft.Extensions.Options;
using Entities = Authorization.DataAccess.Entities;
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

        await using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QueryFirstOrDefaultAsync<Entities.User>(
            CreateCommand(_options.GetUserProcedure, user, cancellationToken));

        return entity is null ? null : ToModel(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Models.Profile>> GetProfilesForUserAsync(Models.User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Profile>(
            CreateCommand(_options.GetProfilesProcedure, user, cancellationToken));

        return entities.Select(ToModel).ToList();
    }

    private static CommandDefinition CreateCommand(string procedure, Models.User user, CancellationToken cancellationToken) =>
        new(procedure,
            new { user.Email, user.UserName },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

    private static Models.User ToModel(Entities.User entity) =>
        new() { Id = entity.Id, UserName = entity.UserName, Email = entity.Email };

    private static Models.Profile ToModel(Entities.Profile entity) =>
        new() { Id = entity.Id, Name = entity.Name };
}
