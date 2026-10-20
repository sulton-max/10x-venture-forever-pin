namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Represents a guest owner and the protected capability carried by its client.</summary>
public sealed record GuestClient
{
    /// <summary>Gets the API client carrying the protected guest cookie.</summary>
    public required HttpClient Client { get; init; }

    /// <summary>Gets the resolved guest owner identifier.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the protected cookie value, distinct from the owner identifier.</summary>
    public required string Cookie { get; init; }
}
