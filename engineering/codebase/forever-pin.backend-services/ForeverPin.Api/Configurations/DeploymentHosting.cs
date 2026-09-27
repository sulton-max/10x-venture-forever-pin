using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Net;
using ForeverPin.Persistence.DataContexts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace ForeverPin.Api.Configurations;

/// <summary>Registers persistent cookie keys, trusted proxies, and readiness probes.</summary>
public static class DeploymentHosting
{
    /// <summary>Applies host-owned deployment settings without trusting arbitrary forwarded headers.</summary>
    public static WebApplicationBuilder AddDeploymentHosting(this WebApplicationBuilder builder)
    {
        builder.Services.AddDataProtection().SetApplicationName("ForeverPin");
        builder.Services.AddOptions<KeyManagementOptions>()
            .Configure<IConfiguration, IHostEnvironment, ILoggerFactory>((options, configuration, environment, logging) =>
            {
                var keyPath = configuration["DataProtection:KeyPath"];
                if (!string.IsNullOrWhiteSpace(keyPath))
                    options.XmlRepository = new FileSystemXmlRepository(new DirectoryInfo(keyPath), logging);
                else if (!environment.IsDevelopment())
                    throw new InvalidOperationException("Production requires DataProtection:KeyPath for persistent identity cookies.");
            })
            .ValidateOnStart();

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
