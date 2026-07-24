using Authorization.Common;
using Entities = Authorization.Abstractions.Entities;
using Models = Authorization.Abstractions.Models;

namespace Authorization.UnitTests;

public class ConverterTests
{
    [Fact]
    public void ConvertList_MapsEveryElement()
    {
        var source = new[]
        {
            new Entities.Profile { Id = 1, Name = "admin" },
            new Entities.Profile { Id = 2, Name = "user" },
        };

        var result = Converter.ConvertList<Entities.Profile, Models.Profile>(source);

        Assert.Equal(2, result.Count);
        Assert.Equal("admin", result[0].Name);
        Assert.Equal("user", result[1].Name);
    }

    [Fact]
    public void ConvertList_EmptySource_ReturnsEmpty()
    {
        var result = Converter.ConvertList<Entities.Profile, Models.Profile>(Array.Empty<Entities.Profile>());
        Assert.Empty(result);
    }

    [Fact]
    public void ConvertList_NullSource_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => Converter.ConvertList<Entities.Profile, Models.Profile>(null!));
    }

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var original = new Models.User { Id = Guid.NewGuid(), UserName = "jdoe" };

        var clone = Converter.Clone(original);

        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(original.UserName, clone!.UserName);
    }
}
