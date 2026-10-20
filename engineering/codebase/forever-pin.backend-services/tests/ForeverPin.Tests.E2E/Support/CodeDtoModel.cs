namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>CodeModel</c>.</summary>
public sealed record CodeDtoModel
{
    public Guid Id { get; init; }
    public string? Slug { get; init; }
    public string? ShortUrl { get; init; }
    public string Name { get; init; } = "";
    public string BarcodeFormat { get; init; } = "";

    /// <summary>Gets how the symbol resolves, either <c>static</c> or <c>dynamic</c>.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Gets the kind of content every rule of this code carries (e.g. <c>url</c>, <c>wifi</c>).</summary>
    public string ContentType { get; init; } = "";

    public bool IsActive { get; init; }
    public long ScanCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves.</summary>
    public IReadOnlyList<RuleDtoModel> Rules { get; init; } = [];
}
