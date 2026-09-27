namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Provides product hosts and database reset between E2E tests.</summary>
public abstract class E2EBase(AppFixture fixture) : IAsyncLifetime
{
    /// <summary>Gets the shared app fixture.</summary>
    protected AppFixture Fixture { get; } = fixture;

    /// <summary>Gets an API client with no identity cookie.</summary>
    protected HttpClient AnonymousClient => Fixture.CreateApiClient();

    /// <summary>Gets a redirect client that preserves redirect responses.</summary>
    protected HttpClient RedirectClient => Fixture.CreateRedirectClient();

    /// <summary>Provisions a guest carrying an authenticated capability.</summary>
    protected Task<GuestClient> CreateGuestClientAsync() => Fixture.CreateGuestClientAsync();

    /// <summary>Resets product rows and the fake external billing broker.</summary>
    public async Task InitializeAsync()
    {
        await Fixture.ResetAsync();
        Fixture.Gateway.Reset();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
