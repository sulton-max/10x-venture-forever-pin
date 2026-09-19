using System.Net;
using ForeverPin.Persistence.DataContexts;
using Microsoft.AspNetCore.HttpOverrides;

namespace ForeverPin.Redirect.Api.Configurations;

/// <summary>Registers trusted deployment proxies and readiness probes.</summary>
public static class DeploymentHosting
{
    /// <summary>Applies host-owned deployment settings without trusting arbitrary forwarded headers.</summary>
    public static WebApplicationBuilder AddDeploymentHosting(this WebApplicationBuilder builder)
    {
        var proxies = builder.Configuration.GetSection("Deployment:TrustedProxies").Get<string[]>() ?? [];
        builder.Services.PostConfigure<ForwardedHeadersOptions>(options =>
        {
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            options.KnownProxies.Add(IPAddress.Loopback);
            options.KnownProxies.Add(IPAddress.IPv6Loopback);
            foreach (var proxy in proxies)
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            options.ForwardLimit = 1;
        });
        builder.Services.AddHealthChecks().AddCheck<DatabaseReadinessCheck>("database");
        return builder;
    }
}
