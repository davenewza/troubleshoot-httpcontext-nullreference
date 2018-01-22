using Microsoft.ApplicationInsights;

namespace Api.Middleware.Telemetry
{
    public class Telemetry
    {
        public TelemetryClient Client { get; }

        public Telemetry()
        {
            this.Client = new TelemetryClient();
        }

        public TelemetryDependency TrackAsDependency(string name, bool alwaysWriteToDebug = false)
        {
            return new TelemetryDependency(Client, name, alwaysWriteToDebug);
        }
    }
}