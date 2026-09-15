using ForeverPin.Common.Domain.Codes.Content.Wifi.Enums;
using ForeverPin.Domain.Codes.Content.Wifi.Extensions;

namespace ForeverPin.Domain.Codes.Content.Wifi.Models;

/// <summary>Represents the credentials of a Wi-Fi network.</summary>
public sealed record WifiContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the network name.</summary>
    public required string Ssid { get; init; }

    /// <summary>Gets the pre-shared key.</summary>
    public string? Password { get; init; }

    /// <summary>Gets the authentication scheme.</summary>
    public required WifiEncryption Encryption { get; init; }

    /// <summary>Gets whether the network withholds its name from beacon frames.</summary>
    public bool Hidden { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
