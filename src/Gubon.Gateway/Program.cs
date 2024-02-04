using FluentValidation;
using FreeRedis;
using Gubon.Gateway;
using Gubon.Gateway.Authorization;
using Gubon.Gateway.Middleware;
using Gubon.Gateway.Utils.Config;
using Gubon.Gateway.Store;
using Gubon.Gateway.Store.FreeSql;
using Gubon.Gateway.Store.FreeSql.Management;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Gubon.Gateway.Store.FreeSql.Validate;
using Gubon.Gateway.TransformFactory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Health;
using Gubon.Gateway.Middlewares;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
//WebApplicationOptions weboptions = new WebApplicationOptions() { WebRootPath = "wwwroot2" ,Args = args};
//var builder = WebApplication.CreateBuilder(weboptions);

// 添加验证器
//builder.Services.AddSingleton<IValidator<Cluster>, ClusterValidator>();
//builder.Services.AddSingleton<IValidator<ProxyRoute>, ProxyRouteValidator>();

//验证设置
var gubonSettings = builder.Configuration.GetSection("GubonSettings").Get<GubonSettings>();
AppProvider.Load(gubonSettings);

if (gubonSettings is null)
{
    throw new Exception("appsettings.json not contain section GubonSettings");
}
else
{
    GubonInfo.Instance.ClusterName = gubonSettings.GubonHttpCounter.ClusterName;
    GubonInfo.Instance.ServiceName = gubonSettings.GubonHttpCounter.ServiceName;
    GubonInfo.Instance.DisplayName = gubonSettings.GubonHttpCounter.DisplayName;
    GubonInfo.Instance.ApiUrl = gubonSettings.GubonHttpCounter.ApiUrl;
    GubonInfo.Instance.StartTime = DateTime.Now;
    GubonInfo.Instance.ReloadTime = DateTime.Now;
}
builder.Services.Configure<GubonSettings>(builder.Configuration.GetSection("GubonSettings"));
builder.Services.AddSingleton(gubonSettings);

//初始化日志
SerilogConfiguration.Init(gubonSettings.SearilogConfig);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddMemoryCache();

builder.Services.AddSerilog();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        // options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
    });



//定义 Redis


if (gubonSettings.GubonHttpCounter.StoreInRedis)
{
    IRedisClient cli = new RedisClient(gubonSettings.DataBase.RedisConn);
    builder.Services.AddSingleton(cli);
    //连接 Redis 并注册数据
    try
    {
        if (!String.IsNullOrEmpty(cli.Ping()))
        {
            var gatewayInfo = GubonInfo.Instance;
            //注册 Service
            cli.HSet($"{gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayServices}", gubonSettings.GubonHttpCounter.ServiceName,
                JsonConvert.SerializeObject(gatewayInfo));
            //清空 Service 统计数据
            var prefix = $"{gubonSettings.GubonHttpCounter.ClusterName}:{gatewayInfo.ServiceName}";
            cli.Del(
                $"{prefix}:{RedisKey.GubonCounterTotal}",
                $"{prefix}:{RedisKey.GubonCounterRoutes}",
                $"{prefix}:{RedisKey.GubonCounterRequests}",
                $"{prefix}:{RedisKey.GubonCounterDestinations}");
        }
    }
    catch { }
}
else
{
    //即使没有使用redis，也要注入一个IRedisClient，不然系统报错
    IRedisClient temp = new RedisClient("");
    builder.Services.AddSingleton(temp);
}

//定义 freeSql
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(Enum.Parse<FreeSql.DataType>(gubonSettings.DataBase.DBType, ignoreCase: true), gubonSettings.DataBase.DBConn)
    .Build();
builder.Services.AddSingleton<IFreeSql>(fsql);
builder.Services.AddFreeRepository(null, typeof(Gubon.Gateway.Store.FreeSql.Models.Cluster).Assembly);

//添加 集群、路由管理器
builder.Services.AddTransient<IClusterManagement, ClusterManagement>();
builder.Services.AddTransient<IProxyRouteManagement, ProxyRouteManagement>();



builder.Services.AddRateLimiter(p =>
{
    p.AddPolicy("Concurrency", context => RateLimitPartition.GetConcurrencyLimiter(
        partitionKey: context.Connection.RemoteIpAddress,
        factory: partitin => new ConcurrencyLimiterOptions
        {
            PermitLimit = 3,
            QueueLimit = 3,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));
    p.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later ", cancellationToken: token);
    };
});

//添加反向代理 YARP
var RPBuilder = builder.Services.AddReverseProxy()
   .LoadFromFreeSql()
    //.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        // handler.ConnectTimeout = TimeSpan.FromSeconds(2);
    })
    .AddTransformFactories();

// 设置 OpenTelemetry


if (gubonSettings.OpenTelemetry.TracingEnable)
{
    var OpenTelemetryBuilder = builder.Services.AddOpenTelemetry()
        .WithTracing(tracerProviderBuilder =>
            tracerProviderBuilder
                .AddSource(DiagnosticsConfig.ActivitySource.Name)
                .ConfigureResource(resource => resource
                    .AddService(DiagnosticsConfig.ServiceName))
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(gubonSettings.OpenTelemetry.Tracing.Endpoint ?? "");
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                    opt.Headers = gubonSettings.OpenTelemetry.Tracing.Headers;
                    //opt.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                })
                );
    if (gubonSettings.OpenTelemetry.MetricsEnable)
    {
        OpenTelemetryBuilder.WithMetrics(metricsProviderBuilder =>
                metricsProviderBuilder
                    .ConfigureResource(resource => resource
                        .AddService(DiagnosticsConfig.ServiceName))
                      .AddMeter(DiagnosticsConfig.Meter.Name)
                    .AddAspNetCoreInstrumentation()
                      .AddOtlpExporter(opt =>
                      {
                          opt.Endpoint = new Uri(gubonSettings.OpenTelemetry.Metrics.Endpoint ?? "");
                          opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                          opt.Headers = gubonSettings.OpenTelemetry.Metrics.Headers;
                          // opt.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                      }));
    }
}

#region HealthCheck 
// Active
// ConsecutiveFailuresHealthPolicy
// Metadata : ConsecutiveFailuresHealthPolicy.Threshold  #连续失败次数，默认2

// Passive
// TransportFailureRateHealthPolicy
builder.Services.Configure<TransportFailureRateHealthPolicyOptions>(o =>
{
    o.DetectionWindowSize = TimeSpan.FromSeconds(60); //统计多少秒内的失败次数
    o.MinimalTotalCountThreshold = 10;   // 前面 X 个请求不进行失败检测
    o.DefaultFailureRateLimit = 0.3;    //失败率  metadata :TransportFailureRateHealthPolicy.RateLimit 
});
#endregion

#region jwt
// configure strongly typed settings object

// configure DI for application services
builder.Services.AddScoped<IJwtUtils, JwtUtils>();
#endregion

// FluentValidate

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
builder.Services.AddValidatorsFromAssemblyContaining<ClusterValidator>();

builder.WebHost.ConfigureKestrel(o =>
{
    o.AddServerHeader = false;
    o.Limits.MaxRequestBodySize = 1_000_000_000;
});


var app = builder.Build();

// 配置静态管理网站
if (gubonSettings.AdminWebSite.Enabled)
{
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(
            string.IsNullOrWhiteSpace(gubonSettings.AdminWebSite.ContentRootPath) ? app.Environment.ContentRootPath : gubonSettings.AdminWebSite.ContentRootPath
            , gubonSettings.AdminWebSite.Folder)),
        RequestPath = new PathString(gubonSettings.AdminWebSite.RequestPath),
    });
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => {
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        //.AllowCredentials()
        ;
});

app.UseRequestBodySizeLimiter();
//app.UseHttpsRedirection();
app.UseMiddleware<JwtMiddleware>();

app.UseAuthorization();


app.UseGubonExceptionHandler(gubonSettings);
app.UseGubontatusCodePagesHandler(gubonSettings);
app.UseServerHeader(gubonSettings.ServerName);

app.MapControllers();


app.MapReverseProxy(proxyPipeline => {

    if (gubonSettings != null)
    {
        proxyPipeline.UseGubonHttpLog(gubonSettings);
        proxyPipeline.UseGubonErrorHandler(gubonSettings);
    }
    proxyPipeline.UseGubonHttpCounter();

    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UsePassiveHealthChecks();
});

// 监听 redis 重新加载

if (gubonSettings.GubonHttpCounter.StoreInRedis)
{
    var iRedisClient = app.Services.GetService(typeof(IRedisClient)) as IRedisClient;
    if (iRedisClient != null)
    {
        iRedisClient.Subscribe($"{gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayReloadConfig}", (chan, data) =>
        {
            // 收到订阅信息，重新加载配置数据
            if (gubonSettings.GubonHttpCounter.ServiceName != data.ToString())
            {
                var store = app.Services.GetService(typeof(IReverseProxyStore)) as IReverseProxyStore;
                if (store != null)
                {
                    store.Reload();
                }
            }

            //更新加载配置的时间
            var info = GubonInfo.Instance;
            var gatewayString = iRedisClient.HGet($"{gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayServices}", gubonSettings.GubonHttpCounter.ServiceName);
            if (gatewayString != null)
            {
                try
                {
                    var gatewayInfo = JsonConvert.DeserializeObject<GateWayInfo>(gatewayString);
                    if (gatewayInfo != null)
                    {
                        info = gatewayInfo;
                    }
                }
                catch { }
            }
            info.ReloadTime = DateTime.Now;
            iRedisClient.HSet($"{gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayServices}", gubonSettings.GubonHttpCounter.ServiceName,
            JsonConvert.SerializeObject(info));

        });
    }
}

// IHostApplicationLifetime
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine($"{GubonInfo.Instance.DisplayName} 启动完成");
});
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine($"{GubonInfo.Instance.DisplayName} 正在关闭");
});
lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine($"{GubonInfo.Instance.DisplayName} 已经关闭");
});

app.Run();

Log.CloseAndFlush();