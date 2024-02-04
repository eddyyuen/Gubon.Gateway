using FreeRedis;
using Gubon.Gateway.Utils.Config;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Concurrent;

namespace Gubon.Gateway.Middleware.ContextStates
{
    public class RecordDesinationsStates
    {
        private readonly RequestDelegate _next;
        // Supplied via DI
        private readonly ILogger<RecordDesinationsStates> _logger;
        private readonly GubonSettings _gubonSettings;
        private readonly IRedisClient _redisClient;


        public RecordDesinationsStates(RequestDelegate next, ILogger<RecordDesinationsStates> logger, GubonSettings gubonSettings, IRedisClient redisClient)
        {
            _logger = logger;
            _next = next;
            _gubonSettings = gubonSettings;
            _redisClient = redisClient;

        }

        /// <summary>
        /// Entrypoint for being called as part of the request pipeline
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Call the next steps in the middleware, including the proxy
            await _next(context);
            
            if (_gubonSettings.GubonHttpCounter.StoreInRedis)
            {
                RedisStore(context);
            }
            else
            {
                LocalStore(context);
            }
        }

        public void LocalStore(HttpContext context)
        {
            var code = context.Response.StatusCode;
            StaticDestinationsStates.current.counter_all.Increment();
            switch (code)
            {
                case >= 500:
                    StaticDestinationsStates.current.counter_5xx.Increment();
                    break;
                case >= 400:
                    StaticDestinationsStates.current.counter_4xx.Increment();
                    break;
                case >= 300:
                    StaticDestinationsStates.current.counter_3xx.Increment();
                    break;
                case >= 200:
                    StaticDestinationsStates.current.counter_2xx.Increment();
                    break;
            }

            var proxyFeature = context.GetReverseProxyFeature();
            var destAddress = proxyFeature.ProxiedDestination?.Model.Config.Address;
            var routeId = proxyFeature.Route.Config.RouteId;
            var requestPath = $"{context.Request.Host}{context.Request.Path}";

            IncrementLocal(StaticDestinationsStates.current._routes, routeId, code);
            IncrementLocal(StaticDestinationsStates.current._destinations, destAddress, code);
            IncrementLocal(StaticDestinationsStates.current._request, requestPath, code);
        
        }

        private void IncrementLocal(ConcurrentDictionary<string, StateAtomicCounter> target, string? key, int code)
        {
            if (string.IsNullOrEmpty(key)) return;

            var exists = target.TryGetValue(key, out var atomicCounterRequest);
            if (!exists || atomicCounterRequest is null)
            {
                
                atomicCounterRequest = new StateAtomicCounter();
            }

            switch (code)
            {
                case >= 500:
                    atomicCounterRequest.AtomicCounter5xx.Increment();
                    break;
                case >= 400:
                    atomicCounterRequest.AtomicCounter4xx.Increment();
                    break;
                case >= 300:
                    break;
                case >= 200:
                    atomicCounterRequest.AtomicCounter2xx.Increment();
                    break;
            }
            switch (code)
            {
                case 400:
                    atomicCounterRequest.AtomicCounter400.Increment();
                    break;
                case 401:
                    atomicCounterRequest.AtomicCounter401.Increment();
                    break;
                case 403:
                    atomicCounterRequest.AtomicCounter403.Increment();
                    break;
                case 405:
                    atomicCounterRequest.AtomicCounter405.Increment();
                    break;
                case 500:
                    atomicCounterRequest.AtomicCounter500.Increment();
                    break;
                case 502:
                    atomicCounterRequest.AtomicCounter502.Increment();
                    break;
                case 503:
                    atomicCounterRequest.AtomicCounter503.Increment();
                    break;
                case 504:
                    atomicCounterRequest.AtomicCounter504.Increment();
                    break;
            }
            if (!exists)
            {
                target.TryAdd(key, atomicCounterRequest);
            }

        }
        public void RedisStore(HttpContext context)
        {
            var prefix = $"{_gubonSettings.GubonHttpCounter.ClusterName}:{_gubonSettings.GubonHttpCounter.ServiceName}";
            var code = context.Response.StatusCode;
            StaticDestinationsStates.current.counter_all.Increment();
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterTotal}", "all", 1L);

            var proxyFeature = context.GetReverseProxyFeature();
            var destAddress = proxyFeature.ProxiedDestination?.Model.Config.Address;
            var routeId = proxyFeature.Route.Config.RouteId;
            var requestPath = $"{context.Request.Host}{context.Request.Path}";

          
            string CodeTag = "1xx";
            switch (code)
            {                
                case >= 500:
                    CodeTag = "5xx";
                    break;
                case >= 400:
                    CodeTag = "4xx";
                    break;
                case >= 200:
                    CodeTag = "2xx";
                    break;
            }

            

            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterTotal}", $"{code}", 1L);
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterTotal}", $"{CodeTag}", 1L);
            //Routes
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterRoutes}", $"{code}|{routeId}", 1L);
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterRoutes}", $"{CodeTag}|{routeId}", 1L);
            //requestPath
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterRequests}", $"{code}|{requestPath}", 1L);
            _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterRequests}", $"{CodeTag}|{requestPath}", 1L);
            //Destinations
            if (destAddress is not null)
            {
                _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterDestinations}", $"{code}|{destAddress}", 1L);
                _redisClient.HIncrByAsync($"{prefix}:{RedisKey.GubonCounterDestinations}", $"{CodeTag}|{destAddress}", 1L);
            }
        }
    }
}
