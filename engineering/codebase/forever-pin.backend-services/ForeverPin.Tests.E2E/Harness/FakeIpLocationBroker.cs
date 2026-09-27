using System.Net;
using WoW.Two.Sdk.Backend.Beta.Geo.IpLocation.Brokers;
using WoW.Two.Sdk.Backend.Beta.Geo.IpLocation.Models;

namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Places one client address in Australia, standing in for the downloaded DB-IP database.</summary>
public sealed class FakeIpLocationBroker : IIpLocationBroker
{
    /// <summary>Holds the client address this fake locates in Australia.</summary>
    public static readonly IPAddress AustralianClient = IPAddress.Parse("1.2.3.4");

    /// <inheritdoc />
    public IpLocationModel? Locate(IPAddress address) =>
        address.Equals(AustralianClient) ? new IpLocationModel { CountryCode = "AU", CountryName = "Australia" } : null;
}
