using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Api.Middleware.Telemetry
{
    public class BuildVersioningTelemetryInitialiser : ITelemetryInitializer
    {
        public string BuildVersion { get; }

        public BuildVersioningTelemetryInitialiser(IOptions<BuildInfoSettings> buildInfoSettings)
        {
            BuildVersion = buildInfoSettings.Value.Version;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = BuildVersion;
        }
    }
}