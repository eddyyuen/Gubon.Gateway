
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Utils.HttpContextHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace Gubon.Gateway.Middleware.CustomMiddleware
{
    public class ErrorHandleMiddleware
    {

        private readonly RequestDelegate _next;
        // Supplied via DI
        private readonly ILogger<ErrorHandleMiddleware> _logger;
        private readonly IFreeSql _ifreeSql;
        public ErrorHandleMiddleware(RequestDelegate next, ILogger<ErrorHandleMiddleware> logger, IFreeSql ifreeSql)
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


            var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();
            if (string.IsNullOrEmpty(availableDestinationsFeature?.Route?.Config?.RouteId))
            {
                LogRouteError(context);
            }

            // Call the next steps in the middleware, including the proxy
            await _next(context);

            //是否有异常，并存储
            var errorFeature = context.GetForwarderErrorFeature();
            if (errorFeature is not null && errorFeature.Error != ForwarderError.None)
            {
                var err = new ErrorRecord()
                {
                    context = context,
                    error = errorFeature.Error,
                    exception = errorFeature.Exception,
                };
               
                await Task.Factory.StartNew((err) =>
                {
                    var errorRecord = (ErrorRecord?)err;
                    if (errorRecord is not null && errorRecord?.context !=null) {
                        LogError(errorRecord.context, errorRecord.error, errorRecord.exception);
                    }
                }, err);

                context.Items["ErrorHandleMiddleware"] = true;
                
            }

            // Called after the other middleware steps have completed
            // Write the info to the console via ILogger. In a production scenario you probably want
            // to write the results to your telemetry systems directly.
            // _logger.LogInformation("PerRequestMetrics: " + metrics.ToJson());
        }
        private record ErrorRecord
        {
            public HttpContext? context;
            public ForwarderError error;
            public Exception? exception;
        }
        private async void LogError(HttpContext context,ForwarderError error, Exception? exception)
        {      
            _logger.LogError("ErrorType:{ErrorType},Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize},StatusCode:{StatusCode},Error:{Error},Exception:{exception}"
           , "ForwarderError", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method,
           RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength, context.Response.StatusCode, error, exception);

            try
            {
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
                logs.Errors = error.ToString();
                logs.Exception = exception?.ToString();
                logs.RequestPath = context.Request.Path;
                logs.Querystring = context.Request.QueryString.ToString();
                logs.Host = context.Request.Host.ToString();


                //获取 body
               
                if (context.Request.ContentLength > 0)
                { 
                    context.Request.EnableBuffering();
                    context.Request.Body.Position = 0;
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var data = reader.ReadToEndAsync().Result;
                    context.Request.Body.Position = 0;
                    logs.RequestBody = data;
                }

                if (context.Response.ContentLength > 0 && context.Response.Body.CanRead && context.Response.Body.CanSeek)
                {
                    context.Response.Body.Position = 0;
                    using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
                    var data = reader.ReadToEndAsync().Result;
                    context.Response.Body.Position = 0;
                    logs.ResponseBody = data;
                }

                var ret = await _ifreeSql.GetRepository<Logs>().InsertAsync(logs);
            }
            catch { }
        }
        private void LogRouteError(HttpContext context,string error="404NotFound")
        {
            //_logger.LogWarning($@"Host:{context.Request.Host},Path:{context.Request.Path},QueryString:{context.Request.QueryString},Method:{context.Request.Method},Error:{error}");
            _logger.LogError("ErrorType:{ErrorType}Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize},StatusCode:{StatusCode},Error:{error}"
            ,"RouteError", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength, context.Response.StatusCode, error);
        }
    }
}
