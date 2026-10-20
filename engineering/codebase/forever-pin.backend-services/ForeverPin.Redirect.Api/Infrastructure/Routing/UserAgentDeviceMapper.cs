using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Redirect.Api.Application.Routing.Services;
using WoW.Two.Sdk.Beta.Be.Web.RequestContext.Models;
using SdkDeviceMapper = WoW.Two.Sdk.Beta.Be.Web.RequestContext.Mappers.UserAgentDeviceMapper;

namespace ForeverPin.Redirect.Api.Infrastructure.Routing;

/// <summary>Maps SDK request-device classification into the product's persisted device vocabulary.</summary>
public sealed class UserAgentDeviceMapper : IDeviceMapper
{
    /// <inheritdoc />
    public DeviceType Resolve(string? userAgent) => SdkDeviceMapper.Map(userAgent) switch
    {
        UserAgentDevice.Ios => DeviceType.Ios,
        UserAgentDevice.Android => DeviceType.Android,
        UserAgentDevice.Desktop => DeviceType.Desktop,
        UserAgentDevice.Bot => DeviceType.Bot,
        _ => DeviceType.Unknown,
    };
}
