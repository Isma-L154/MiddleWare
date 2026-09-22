using Authorization.Abstractions.Options;
using Authorization.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Authorization.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void StartupValidation_MissingConnectionString_Throws()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>());

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.Contains("ConnectionStrings", exception.Message);
    }

    [Fact]
    public void StartupValidation_BlankClaimType_Throws()
    {
        using var provider = BuildProvider(ValidConnectionString, options => options.UserNameClaimType = " ");

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void StartupValidation_ValidConfiguration_Passes()
    {
        using var provider = BuildProvider(ValidConnectionString);

        var exception = Record.Exception(() => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.Null(exception);
    }

    private static readonly Dictionary<string, string?> ValidConnectionString = new()
    {
        ["ConnectionStrings:SecurityDb"] = "Server=localhost;Database=Security;",
    };

    private static ServiceProvider BuildProvider(
        Dictionary<string, string?> settings,
        Action<ClaimsEnrichmentOptions>? configure = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddAuthorizationClaims(configure)
            .BuildServiceProvider();
    }
}
