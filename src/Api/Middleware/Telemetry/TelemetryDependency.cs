using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Diagnostics;

namespace Api.Middleware.Telemetry
{
    public class TelemetryDependency : IDisposable
    {
        public string Name { get; }

        public Stopwatch Timer { get; }

        public bool AlwaysWriteToDebug { get; }

        private TelemetryClient telemetryClient;

        public TelemetryDependency(TelemetryClient client, string name, bool alwaysWriteToDebug = false)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Timer = Stopwatch.StartNew();
            AlwaysWriteToDebug = alwaysWriteToDebug;
            telemetryClient = client;
        }

        public void Dispose()
        {
            Timer.Stop();

            if (!TelemetryConfiguration.Active.DisableTelemetry)
            {
                telemetryClient.TrackDependency(Name, String.Empty, DateTime.UtcNow.Subtract(Timer.Elapsed), Timer.Elapsed, true);
            }

            if (AlwaysWriteToDebug || !TelemetryConfiguration.Active.DisableTelemetry)
            {
                Debug.WriteLine($"[ {Name}; {Timer.ElapsedMilliseconds}ms ]");
            }
        }
    }
}