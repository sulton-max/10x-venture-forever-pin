extern alias apihost;

using System.Net;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using ForeverPin.Tests.E2E.Harness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;
using RateLimitPolicies = apihost::ForeverPin.Api.Configurations.RateLimitPolicies;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>Verifies the management host's request budgets: every named budget is configured and enforced.</summary>
[Collection(AppCollection.Name)]
public sealed class RateLimitTests(AppFixture fixture) : E2EBase(fixture)
{
    private static readonly Assembly ApiAssembly = typeof(RateLimitPolicies).Assembly;

    [Fact]
    public void Controllers_ShouldBudgetSessionsPreviewsWritesAndImages()
    {
        var budgets = ApiAssembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Select(method => (Action: $"{method.DeclaringType!.Name}.{method.Name}",
                Policy: method.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName))
            .Where(entry => entry.Policy is not null)
            .ToDictionary(entry => entry.Action, entry => entry.Policy);

        budgets.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["IdentityController.Guest"] = RateLimitPolicies.Sessions,
            ["AuthController.Google"] = RateLimitPolicies.Sessions,
            ["CodesController.Preview"] = RateLimitPolicies.Previews,
            ["CodesController.Create"] = RateLimitPolicies.Writes,
            ["CodesController.UpdateById"] = RateLimitPolicies.Writes,
            ["CodesController.SetActiveById"] = RateLimitPolicies.Writes,
            ["CodesController.DeleteById"] = RateLimitPolicies.Writes,
            ["CodesController.GetImageById"] = RateLimitPolicies.Images,
        });
    }

    [Theory]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Development.json")]
    public void Settings_ShouldDeclareEveryBudget(string file)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(ApiProjectDirectory(), file)));
        var policies = document.RootElement.GetProperty("RateLimits").GetProperty("Policies");

        foreach (var name in RateLimitPolicies.All.Append("api"))
            policies.TryGetProperty(name, out _).Should().BeTrue($"{file} must declare the '{name}' budget");
    }

    [Fact]
    public async Task Sessions_ShouldAnswer429_WhenTheBudgetIsSpent()
    {
        using var host = Fixture.ApiHost.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimits:Policies:sessions:PermitLimit", "2"));
        using var client = host.CreateSpaClient(AppFixture.TokenPath);

        for (var attempt = 0; attempt < 2; attempt++)
            (await client.PostAsync("/api/identity/guest", content: null)).StatusCode.Should().Be(HttpStatusCode.OK);
        using var rejected = await client.PostAsync("/api/identity/guest", content: null);

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await rejected.Content.ReadAsStringAsync()).Should().Contain("TooManyRequests");
    }

    /// <summary>Finds the management host's project directory from the test output directory.</summary>
    private static string ApiProjectDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "ForeverPin.Api");
            if (File.Exists(Path.Combine(candidate, "appsettings.json"))) return candidate;
        }

        throw new DirectoryNotFoundException("ForeverPin.Api was not found above the test output directory.");
    }
}
