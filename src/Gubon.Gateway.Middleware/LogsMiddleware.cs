using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Utils.HttpContextHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Buffers;

namespace Gubon.Gateway.Middleware
{
    public class LogsMiddleware
    {

        private readonly RequestDelegate _next;
        // Supplied via DI
        private readonly ILogger<LogsMiddleware> _logger;
        private readonly IFreeSql _ifreeSql;
        public LogsMiddleware(RequestDelegate next, ILogger<LogsMiddleware> logger, IFreeSql ifreeSql)
        {
            _logger = logger;
            _next = next; 
            _ifreeSql = ifreeSql;
        }

        /// <summary>
        /// Entrypoint for being called as part of the request pipeline
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            //var useDebugDestinations = context.Request.Headers.TryGetValue("DEBUG_HEADER", out var headerValues) && headerValues.Count == 1 && headerValues[0] == "DEBUG_VALUE";

            //// The context also stores a ReverseProxyFeature which holds proxy specific data such as the cluster, route and destinations
            //var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();
            //var filteredDestinations = new List<DestinationState>();

            //// Filter destinations based on criteria
            //foreach (var d in availableDestinationsFeature.AvailableDestinations)
            //{
            //    //Todo: Replace with a lookup of metadata - but not currently exposed correctly here
            //    if (d.DestinationId.Contains("debug") == useDebugDestinations) { filteredDestinations.Add(d); }
            //}
            //availableDestinationsFeature.AvailableDestinations = filteredDestinations;

            //// Important - required to move to the next step in the proxy pipeline
           

           // LogRequest(context);
            // Call the next steps in the middleware, including the proxy
            await _next(context);
            //await LogRespone(context);

            await Task.Factory.StartNew((ctx) =>
            {
                var context = (HttpContext?)ctx;
                if(context != null) { LogRespone(context); }
            }, context);

            // Called after the other middleware steps have completed
            // Write the info to the console via ILogger. In a production scenario you probably want
            // to write the results to your telemetry systems directly.
            // _logger.LogInformation("PerRequestMetrics: " + metrics.ToJson());
        }
       
        private void LogRequest(HttpContext context)
        {
           // _logger.LogDebug($@"Type:{"Request"},Host:{context.Request.Host},Path:{context.Request.Path},QueryString:{context.Request.QueryString},Method:{context.Request.Method}");
            _logger.LogDebug("{Type},Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize}"
          , "Request", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength);
        }
        private  void  LogRespone(HttpContext context)
        {
            
            // _logger.LogDebug($@"Type:{"Respone"}Host:{context.Request.Host},Path:{context.Request.Path},QueryString:{context.Request.QueryString},Method:{context.Request.Method},RessponeCode:{context.Response.StatusCode}");
            _logger.LogDebug("{Type},Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize}"
          , "Request", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength);

        
            _logger.LogDebug("{Type},Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},StatusCode:{StatusCode},BodySize:{BodySize}"
            , "Response", context.Request.Scheme,context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Response.StatusCode,context.Response.ContentLength);
            if (context.Response.StatusCode >= 400)
            {
                //判断是否已经经过错误处理
                if (context.Items.TryGetValue("ErrorHandleMiddleware", out var _)) return;

                //记录日志
                var logs = new Logs();
                logs.Id = 0;
                logs.ResponseTime = DateTime.Now;
                logs.Method = context.Request.Method;
                logs.Scheme = context.Request.Scheme;
                logs.Ip = RemoteIpHelper.GetRemoteIp(context);
                logs.ResponseContentLength = context.Response.ContentLength;
                logs.RequestContentLength = context.Request.ContentLength;
                logs.StatusCode = context.Response.StatusCode;
                logs.ResponseBody = "";
                logs.RequestBody = "";
                logs.Errors = "StatusCode>=400";
                logs.Exception = "";
                logs.RequestPath = context.Request.Path;
                logs.Querystring = context.Request.QueryString.ToString();
                logs.Host = context.Request.Host.ToString();

                //获取 body
               
                if (context.Request.ContentLength> 0)
                {
                    context.Request.EnableBuffering();
                    context.Request.Body.Position = 0;                    
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var data = reader.ReadToEndAsync().Result;
                    context.Request.Body.Position = 0;                 
                    logs.RequestBody = data;
                }
            
                if (context.Response.ContentLength >0 && context.Response.Body.CanRead && context.Response.Body.CanSeek)
                {
                    context.Response.Body.Position = 0;
                    using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
                    var data = reader.ReadToEndAsync().Result;
                    context.Response.Body.Position = 0;
                    logs.ResponseBody = data;
                }
                _ifreeSql.GetRepository<Logs>().InsertAsync(logs);
            }
        }
      
    }
}
