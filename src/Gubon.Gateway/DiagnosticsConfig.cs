using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Gubon.Gateway
{
    public static class DiagnosticsConfig
    {
        public const string ServiceName = "MyService";
        public static ActivitySource ActivitySource = new ActivitySource(ServiceName);

        public static Meter Meter = new(ServiceName);
        public static Counter<long> RequestCounter =
            Meter.CreateCounter<long>("app.request_counter");
    }
}
