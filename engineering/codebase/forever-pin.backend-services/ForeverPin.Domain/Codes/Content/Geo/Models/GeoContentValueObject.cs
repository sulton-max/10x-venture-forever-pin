using System.Globalization;

namespace ForeverPin.Domain.Codes.Content.Geo.Models;

/// <summary>Represents a point on the globe.</summary>
public sealed record GeoContentValueObject : CodeContentValueObject
{
    /// <summary>Holds the payload shape — the scheme, then the pair a comma separates.</summary>
    private const string Payload = "geo:{0},{1}";

    /// <summary>Gets the latitude in degrees, from -90 to 90.</summary>
    public required double Latitude { get; init; }

    /// <summary>Gets the longitude in degrees, from -180 to 180.</summary>
    public required double Longitude { get; init; }

    /// <inheritdoc />
    /// <remarks>Formats both parts invariantly.</remarks>
    public override string Encode() => string.Format(
        CultureInfo.InvariantCulture,
        Payload,
        Latitude,
        Longitude);
}
