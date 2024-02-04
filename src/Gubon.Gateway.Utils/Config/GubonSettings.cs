using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Utils.Config
{
    public static class AppProvider 
    {
        public static GubonSettings GubonSettings { get; set; } = new();
        public static bool Load(GubonSettings? gubonSettings)
        {
            if (gubonSettings == null) { return false; }
            GubonSettings = gubonSettings;
            return true;
        }
    }
    public class GubonSettings
    {
        public string ServerName { get; set; }
        public Database DataBase { get; set; }
        public Searilogconfig SearilogConfig { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public Middlewares Middlewares { get; set; }
        public Gubonexception GubonException { get; set; }
        public Gubonhttpcounter GubonHttpCounter { get; set; }
        public Opentelemetry OpenTelemetry { get; set; }

        public AdminWebSite AdminWebSite { get; set; } = new();

        public long MaxRequestBodySize { get; set; } = 100_000_000;
    }

    public class Database
    {
        public string DBType { get; set; }
        public string DBConn { get; set; }
        public string RedisConn{ get; set; }
    }

    public class Searilogconfig
    {
        public string LogsFolder { get; set; }
        public string LogsFileName { get; set; }
        public int FileSizeLimitMB { get; set; }
        public int RetainedFileCountLimit { get; set; } = 31;

        public int LogEventLevel { get; set; } = 1;
    }

    public class JwtSettings
    {
        public string Secret { get; set; }
        public int ExpiredTime { get; set; }
    }

    public class AdminWebSite
    {
        public bool Enabled { get; set; } = false;
        public string ContentRootPath { get; set; } = string.Empty;
        public string Folder { get; set; } = "views";

        public string RequestPath{ get; set; } = "/__admin";

    }
    public class Middlewares
    {
        public bool UseGubonErrorHandler { get; set; }
        public bool UseGubonHttpLog { get; set; }
        public bool UseGubonExceptionHandler { get; set; }
        public bool UseGubontatusCodePagesHandler { get; set; }
    }

    public class Gubonexception
    {
        public bool UseCustomResponse { get; set; }
        public string ContentType { get; set; }
        public int StatusCode { get; set; }
        public string ResponeBody { get; set; }
    }

    public class Gubonhttpcounter
    {
        public string ClusterName { get; set; }
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public bool StoreInRedis { get; set; }
        public string ApiUrl { get; set; }
    }

    public class Opentelemetry
    {
        public bool TracingEnable { get; set; }
        public bool MetricsEnable { get; set; }
        public Tracing Tracing { get; set; }
        public Metrics Metrics { get; set; }
    }

    public class Tracing
    {
        public string Endpoint { get; set; }
        public string Headers { get; set; }
    }

    public class Metrics
    {
        public string Endpoint { get; set; }
        public string Headers { get; set; }
    }
}
