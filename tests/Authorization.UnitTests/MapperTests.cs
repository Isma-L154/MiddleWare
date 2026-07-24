using Authorization.Common;
using Entities = Authorization.Abstractions.Entities;
using Models = Authorization.Abstractions.Models;

namespace Authorization.UnitTests;

public class MapperTests
{
    [Fact]
    public void Map_CopiesMatchingProperties()
    {
        var id = Guid.NewGuid();
        var source = new Entities.User
        {
            Id = id,
            UserName = "jdoe",
            Email = "jdoe@example.com",
            PasswordHash = "hash",
        };

        var result = Mapper.Map<Entities.User, Models.User>(source);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("jdoe", result.UserName);
        Assert.Equal("jdoe@example.com", result.Email);
        Assert.Equal("hash", result.PasswordHash);
    }

    [Fact]
    public void Map_NullSource_ReturnsDefault()
    {
        var result = Mapper.Map<Entities.User, Models.User>(null);
        Assert.Null(result);
    }

    [Fact]
    public void Map_AppliesTransformAfterCopy()
    {
        var source = new Entities.Profile { Id = 7, Name = "admin" };

        var result = Mapper.Map<Entities.Profile, Models.Profile>(
            source,
            (src, dest) => dest.Name = src.Name!.ToUpperInvariant());

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("ADMIN", result.Name);
    }

    [Fact]
    public void Map_IsConsistentAcrossCachedCalls()
    {
        // Exercises the per-type-pair property-map cache: a second call must
        // produce the same result as the first.
        var first = Mapper.Map<Entities.Profile, Models.Profile>(new Entities.Profile { Id = 1, Name = "a" });
        var second = Mapper.Map<Entities.Profile, Models.Profile>(new Entities.Profile { Id = 2, Name = "b" });

        Assert.Equal(1, first!.Id);
        Assert.Equal("a", first.Name);
        Assert.Equal(2, second!.Id);
        Assert.Equal("b", second.Name);
    }
}
