using Gubon.Gateway.Middleware.CustomMiddleware;
using Serilog.Events;
using Serilog.Filters;
using Serilog;
using Serilog.Formatting.Compact;
using Gubon.Gateway.Middleware;
using Gubon.Gateway.Utils.Config;

namespace Gubon.Gateway
{
    public class SerilogConfiguration
    {
        public SerilogConfiguration() { }
        /// <summary>
        /// 初始化 Serilog 的文件路径等配置
        /// </summary>
        public static void Init(Searilogconfig searilogConfig)
        {

            string LogsFileName = searilogConfig.LogsFileName;
            string LogsFolder = searilogConfig.LogsFolder;
            long LogsFileSize = searilogConfig.FileSizeLimitMB * 1000 * 1000;
            int RetainedFileCountLimit = searilogConfig.RetainedFileCountLimit;
            LogEventLevel logEventLevel() => (LogEventLevel)searilogConfig.LogEventLevel;

            string LogFilePath(string LogEvent) => Path.Combine(LogsFolder, LogEvent, LogsFileName);

            var matchLogs = Matching.FromSource<LogsMiddleware>();
            var matchErroHandle = Matching.FromSource<ErrorHandleMiddleware>();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(matchLogs)
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        LogFilePath("Request"),
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: LogsFileSize,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        restrictedToMinimumLevel: logEventLevel(),
                        retainedFileCountLimit: RetainedFileCountLimit
                        ))
                  .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(matchErroHandle)
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        LogFilePath("Error"),
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: LogsFileSize,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        restrictedToMinimumLevel: logEventLevel(),
                        retainedFileCountLimit: RetainedFileCountLimit
                        )
                   )
                .WriteTo.Logger(lg => lg.Filter.ByExcluding(lf => matchErroHandle(lf) || matchLogs(lf))
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        LogFilePath("Gubon"),
                         logEventLevel(),
                        fileSizeLimitBytes: LogsFileSize,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: RetainedFileCountLimit
                        ))
                .CreateLogger();
        }
    }
}
