using ForeverPin.Api.Configurations;
using WoW.Two.Sdk.Backend.Beta.Observability.Logging.Services;

await new StartupFailureReportingService().RunAsync(async cancellationToken =>
{
    // Build and configure the host.
    var builder = WebApplication.CreateBuilder(args);
    builder.Configure();

    // Build and configure the app.
    var app = builder.Build();
    await app.ConfigureAsync();

    // Run.
    await app.RunAsync(cancellationToken);
});

public partial class Program;
