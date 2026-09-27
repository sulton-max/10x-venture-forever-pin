namespace ForeverPin.Domain.Codes.Content.Geo.Models;

/// <summary>Represents a point on the globe.</summary>
public sealed record GeoContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the latitude in degrees, from -90 to 90.</summary>
    public required double Latitude { get; init; }

    /// <summary>Gets the longitude in degrees, from -180 to 180.</summary>
    public required double Longitude { get; init; }

}
