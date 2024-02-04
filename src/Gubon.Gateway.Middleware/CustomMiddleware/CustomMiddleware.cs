using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Model;

namespace Gubon.Gateway.Middleware.CustomMiddleware
{
    public class CustomMiddleware 
       {

        private readonly RequestDelegate _next;
        // Supplied via DI
        private readonly ILogger<CustomMiddleware> _logger;

        public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        /// <summary>
        /// Entrypoint for being called as part of the request pipeline
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var useDebugDestinations = context.Request.Headers.TryGetValue("DEBUG_HEADER", out var headerValues) && headerValues.Count == 1 && headerValues[0] == "DEBUG_VALUE";

            // The context also stores a ReverseProxyFeature which holds proxy specific data such as the cluster, route and destinations
            var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();
            var filteredDestinations = new List<DestinationState>();

            // Filter destinations based on criteria
            foreach (var d in availableDestinationsFeature.AvailableDestinations)
            {
                //Todo: Replace with a lookup of metadata - but not currently exposed correctly here
                if (d.DestinationId.Contains("debug") == useDebugDestinations) { filteredDestinations.Add(d); }
            }
            availableDestinationsFeature.AvailableDestinations = filteredDestinations;

            // Important - required to move to the next step in the proxy pipeline


            // Call the next steps in the middleware, including the proxy
            await _next(context);

            // Called after the other middleware steps have completed
            // Write the info to the console via ILogger. In a production scenario you probably want
            // to write the results to your telemetry systems directly.
            // _logger.LogInformation("PerRequestMetrics: " + metrics.ToJson());
        }
    }
}
