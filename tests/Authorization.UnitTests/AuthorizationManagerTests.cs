using Authorization.Abstractions.DataAccess;
using Authorization.Abstractions.Models;
using Authorization.Business;
using Moq;

namespace Authorization.UnitTests;

public class AuthorizationManagerTests
{
    [Fact]
    public async Task GetUserAsync_DelegatesToRepository()
    {
        var expected = new User { Id = Guid.NewGuid(), UserName = "jdoe" };
        var repository = new Mock<ISecurityRepository>();
        repository
            .Setup(r => r.GetUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var manager = new AuthorizationManager(repository.Object);

        var result = await manager.GetUserAsync(new User { UserName = "jdoe" });

        Assert.Same(expected, result);
        repository.Verify(r => r.GetUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfilesForUserAsync_DelegatesToRepository()
    {
        var expected = new[] { new Profile { Id = 1, Name = "admin" } };
        var repository = new Mock<ISecurityRepository>();
        repository
            .Setup(r => r.GetProfilesForUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var manager = new AuthorizationManager(repository.Object);

        var result = await manager.GetProfilesForUserAsync(new User { UserName = "jdoe" });

        Assert.Same(expected, result);
    }

    [Fact]
    public void Constructor_NullRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthorizationManager(null!));
    }
}
