using FreeSql;
using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Authentication;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using RouteQueryParameter = Yarp.ReverseProxy.Configuration.RouteQueryParameter;
using SessionAffinityConfig = Gubon.Gateway.Store.FreeSql.Models.SessionAffinityConfig;
using WebProxyConfig = Yarp.ReverseProxy.Configuration.WebProxyConfig;

namespace Gubon.Gateway.Store.FreeSql
{
    public class FreeSqlReverseProxyStore : IReverseProxyStore
    {
        private FreeSqlReloadToken _reloadToken = new FreeSqlReloadToken();
        private IServiceProvider _sp;
        private IMemoryCache _cache;
        private ILogger Logger;

        public event ConfigChangeHandler ChangeConfig;

        public FreeSqlReverseProxyStore(IServiceProvider sp, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<FreeSqlReverseProxyStore>();
            _sp = sp;
            _cache = cache;
            ChangeConfig += ReloadConfig;
        }
        public IProxyConfig GetConfig()
        {
            Logger.LogInformation("GetConfig");
            var exist = _cache.TryGetValue<IProxyConfig>("ReverseProxyConfig", out IProxyConfig? config);
            if (exist)
            {
                return config;
            }
            else
            {
                config = GetFromDb();
                SetConfig(config);

                return config;
            }
        }

        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        public void Reload()
        {
            Logger.LogInformation("ChangeConfig");
            if (ChangeConfig != null)
                ChangeConfig();
        }

        public void ReloadConfig()
        {
            Logger.LogInformation("SetConfig");
            SetConfig();
            Interlocked.Exchange<FreeSqlReloadToken>(ref this._reloadToken,
                new FreeSqlReloadToken()).OnReload();
        }

        private void SetConfig()
        {
            var config = GetFromDb();
            SetConfig(config);
        }
        private void SetConfig(IProxyConfig config)
        {
            _cache.Set("ReverseProxyConfig", config);
        }
        private IProxyConfig GetFromDb()
        {
            var dbContext = _sp.CreateScope().ServiceProvider.GetService<IFreeSql>();
            var newConfig = new StoreProxyConfig();
            if (dbContext is null) { return newConfig; }

            var clusters = dbContext.Select<Cluster>()
                .IncludeMany(c => c.Destinations)
                .Include(c => c.HttpClient)
                .IncludeMany(c => c.Metadatas)
                .Include(c => c.HttpRequest)
                .Include(c => c.SessionAffinity)
                .Include(c => c.HealthCheckConfig.Active)
                .Include(c => c.HealthCheckConfig.Passive)
                .ToList(true);

            var routers = dbContext.Select<ProxyRoute>()
                .Include(r => r.Match)
                .Include(r => r.Cluster)
                .IncludeMany(r => r.Metadatas)
                .IncludeMany(r => r.Transforms)
                .IncludeMany(r => r.Match.QueryParameters)
                .IncludeMany(r => r.Match.Headers)
                .ToList();



            foreach (var section in clusters)
            {
                newConfig.Clusters.Add(CreateCluster(section));
            }

            foreach (var section in routers)
            {
                newConfig.Routes.Add(CreateRoute(section));
            }

            return newConfig;
        }

        private Yarp.ReverseProxy.Configuration.ClusterConfig CreateCluster(Cluster cluster)
        {
            var destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>(StringComparer.OrdinalIgnoreCase);
            foreach (var destination in cluster.Destinations)
            {
                destinations.Add(destination.DestName, CreateDestination(destination));
            }
            var httpRequest = cluster.EnableHttpRequest? CreateProxyRequestConfig(cluster.HttpRequest):null;
            var sessionAffinity = cluster.EnableSessionAffinity ? CreateSessionAffinityOptions(cluster.SessionAffinity) : null;

            return new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = cluster.ClusterName,
                LoadBalancingPolicy = cluster.LoadBalancingPolicy.ReadString(),
                SessionAffinity = sessionAffinity,
                HealthCheck = CreateHealthCheckOptions(cluster.HealthCheckConfig),
                HttpClient = cluster.EnableHttpClient ? CreateHttpClientConfig(cluster.HttpClient) : null,

                Metadata = cluster.Metadatas?.ReadStringDictionary(),
                Destinations = destinations,
                HttpRequest = httpRequest,
            };
        }

        private static Yarp.ReverseProxy.Configuration.RouteConfig CreateRoute(ProxyRoute proxyRoute)
        {
            if (string.IsNullOrEmpty(proxyRoute.RouteName))
            {
                throw new Exception("The route config format has changed, routes are now objects instead of an array. The route id must be set as the object name, not with the 'RouteId' field.");
            }

            if (string.IsNullOrEmpty(proxyRoute.Cluster?.ClusterName))
            {
                throw new Exception("ClusterId can not be empty");
            }

   
            if(!proxyRoute.EnableTransforms) {
                proxyRoute.Transforms = null;
            }

            var routeConfig =  new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = proxyRoute.RouteName,
                Order = proxyRoute.Order,
                ClusterId = proxyRoute.Cluster?.ClusterName,
                MaxRequestBodySize = proxyRoute.MaxRequestBodySize > 0 ? proxyRoute.MaxRequestBodySize : null,
                AuthorizationPolicy = proxyRoute.AuthorizationPolicy.ReadString(),
                CorsPolicy = proxyRoute.CorsPolicy.ReadString(),
                RateLimiterPolicy = proxyRoute.RateLimiterPolicy?.ReadString(),
                Metadata = proxyRoute.Metadatas?.ReadStringDictionary(),
                Transforms = CreateTransforms(proxyRoute.Transforms),
                Match = CreateProxyMatch(proxyRoute.Match),

            };

            return routeConfig;
        }

        private static IReadOnlyList<IReadOnlyDictionary<string, string>>? CreateTransforms(List<Transform>? transforms)
        {
            if (transforms is null || transforms.Count == 0)
            {
                return null;
            }
            var groupTransforms = transforms.OrderBy(t => t.Id).GroupBy(t => t.Type);
            var list = new List<IReadOnlyDictionary<string, string>>();
            foreach (var group in groupTransforms)
            {
                var key = group.Key.ToString();
                Dictionary<string, string> dir = new Dictionary<string, string>();
                foreach (var transform in group)
                {
                    if (transform.Type == TransformType.Custom) //自定义Transforms
                    {
                        if (dir.Count != 0)
                        {
                            list.Add(dir);
                            dir = new Dictionary<string, string>();
                        }
                        dir.Add(transform.Key, transform.Value);
                        list.Add(dir);
                        dir = new Dictionary<string, string>();
                    }
                    else //系统内置 TransForms
                    {
                        if (transform.Key == key)
                        {
                            if (dir.Count != 0)
                                list.Add(dir);
                            dir = new Dictionary<string, string>();
                        }
                        dir.Add(transform.Key, transform.Value);
                    }
                }
                if (dir.Count != 0)
                    list.Add(dir);
            }
            return list;
        }

        private static Yarp.ReverseProxy.Configuration.RouteMatch CreateProxyMatch(ProxyMatch match)
        {
            if (!match.EnableQueryParameters) { match.QueryParameters = null; }
            if (!match.EnableHeaders) { match.Headers = null; }

            return new Yarp.ReverseProxy.Configuration.RouteMatch()
            {
              //  Methods = CreateRouteQueryMethods(match.Methods),
                Methods = match.Methods?.ReadStringArray(),
                Hosts = match.Hosts?.ReadStringArray(),
                Path = match.Path.ReadString(),
                Headers = CreateRouteHeaders(match.Headers),
                QueryParameters = CreateRouteQueryParameters(match.QueryParameters)
            };
        }

        private static IReadOnlyList<Yarp.ReverseProxy.Configuration.RouteHeader>? CreateRouteHeaders(List<Models.RouteHeader>? routeHeaders)
        {
            if (routeHeaders is null || routeHeaders.Count == 0)
            {
                return null;
            }

            return routeHeaders.Select(data => CreateRouteHeader(data)).ToArray();
        }

        private static Yarp.ReverseProxy.Configuration.RouteHeader CreateRouteHeader(Models.RouteHeader routeHeader)
        {
            return new Yarp.ReverseProxy.Configuration.RouteHeader()
            {
                Name = routeHeader.Name,
                Values = routeHeader.Mode != HeaderMatchMode.Exists ? routeHeader.Values.ReadStringArray() : null,
                Mode = routeHeader.Mode,
                IsCaseSensitive = routeHeader.IsCaseSensitive,
            };
        }

        private static IReadOnlyList<RouteQueryParameter>? CreateRouteQueryParameters(IReadOnlyList<Models.RouteQueryParameter>? routeQueryParameters)
        {
            if (routeQueryParameters is null || routeQueryParameters.Count == 0)
            {
                return null;
            }

            return routeQueryParameters.Select(data => CreateRouteQueryParameter(data)).ToArray();
        }
        
  
        private static RouteQueryParameter CreateRouteQueryParameter(Models.RouteQueryParameter routeQueryParameter)
        {
            return new RouteQueryParameter()
            {
                Name = routeQueryParameter.Name,
                Values = routeQueryParameter.Values.ReadStringArray(),
                Mode = routeQueryParameter.Mode,
                IsCaseSensitive = routeQueryParameter.IsCaseSensitive,
            };
        }
        private static Yarp.ReverseProxy.Configuration.SessionAffinityConfig? CreateSessionAffinityOptions(Models.SessionAffinityConfig? sessionAffinityOptions)
        {
            if (sessionAffinityOptions is null )
            {
                return null;
            }

            return new Yarp.ReverseProxy.Configuration.SessionAffinityConfig
            {
                Policy = sessionAffinityOptions.Policy?.ReadString(),
                FailurePolicy = sessionAffinityOptions.FailurePolicy?.ReadString(),
                AffinityKeyName = sessionAffinityOptions.AffinityKeyName,
                Enabled = sessionAffinityOptions.Enabled ,
                Cookie = CreateSessionAffinityCookieConfig(sessionAffinityOptions)
            };
        }

        private static Yarp.ReverseProxy.Configuration.SessionAffinityCookieConfig? CreateSessionAffinityCookieConfig(SessionAffinityConfig? sessionAffinityCookie)
        {
            if (sessionAffinityCookie is null)
            {
                return null;
            }

            return new SessionAffinityCookieConfig
            {
                Path = sessionAffinityCookie.CookiePath?.ReadString(),
                SameSite = sessionAffinityCookie.CookieSameSite,
                HttpOnly = sessionAffinityCookie.CookieHttpOnly,
                MaxAge = sessionAffinityCookie.CookieMaxAge?.ReadTimeSpan(),
                Domain = sessionAffinityCookie.CookieDomain?.ReadString(),
                IsEssential = sessionAffinityCookie.CookieIsEssential,
                SecurePolicy = sessionAffinityCookie.CookieSecurePolicy,
                Expiration = sessionAffinityCookie.CookieExpiration?.ReadTimeSpan()
            };
        }
        private static Yarp.ReverseProxy.Configuration.HealthCheckConfig? CreateHealthCheckOptions(Models.HealthCheckConfig? healthCheckOptions)
        {
            if (healthCheckOptions is null  || (!healthCheckOptions.EnableActive && !healthCheckOptions.EnablePassive))
            {
                return null;
            }
     

            return new Yarp.ReverseProxy.Configuration.HealthCheckConfig
            {
                Passive = CreatePassiveHealthCheckOptions(healthCheckOptions),
                Active = CreateActiveHealthCheckOptions(healthCheckOptions),
                AvailableDestinationsPolicy = healthCheckOptions.AvailableDestinationsPolicy?.ReadString()
            };
        }

        private static Yarp.ReverseProxy.Configuration.PassiveHealthCheckConfig? CreatePassiveHealthCheckOptions(Models.HealthCheckConfig healthCheckOptions)
        {
            if (healthCheckOptions.Passive is null || healthCheckOptions.Passive.Id==0)
            {
                return null;
            }

            return new Yarp.ReverseProxy.Configuration.PassiveHealthCheckConfig
            {
                Enabled = healthCheckOptions.EnablePassive,
                Policy = healthCheckOptions.Passive.Policy.ReadString(),
                ReactivationPeriod = healthCheckOptions.Passive.ReactivationPeriod?.ReadTimeSpan()
            };
        }

        private static Yarp.ReverseProxy.Configuration.ActiveHealthCheckConfig? CreateActiveHealthCheckOptions(Models.HealthCheckConfig healthCheckOptions)
        {
            if (healthCheckOptions.Active is null || healthCheckOptions.Active.Id == 0)
            {
                return null;
            }
           

            return new Yarp.ReverseProxy.Configuration.ActiveHealthCheckConfig
            {
                Enabled = healthCheckOptions.EnableActive,
                Interval = healthCheckOptions.Active?.Interval?.ReadTimeSpan(),
                Timeout = healthCheckOptions.Active?.Timeout?.ReadTimeSpan(),
                Policy = healthCheckOptions.Active?.Policy.ReadString(),
                Path = healthCheckOptions.Active?.Path.ReadString()
            };
        }

        private static Yarp.ReverseProxy.Configuration.HttpClientConfig? CreateHttpClientConfig(Models.HttpClientConfig? proxyHttpClientOptions)
        {
            if (proxyHttpClientOptions is null)
            {
                return null;
            }        

            SslProtocols? sslProtocols = null;
            if (!string.IsNullOrWhiteSpace(proxyHttpClientOptions.SslProtocols))
            {
                foreach (var protocolConfig in proxyHttpClientOptions.SslProtocols.Split(",").Select(s => Enum.Parse<SslProtocols>(s, ignoreCase: true)))
                {
                    sslProtocols = sslProtocols == null ? protocolConfig : sslProtocols | protocolConfig;
                }
            }
            else
            {
                sslProtocols = SslProtocols.None;
            }

            WebProxyConfig? webProxy;
           
            if (proxyHttpClientOptions.WebProxy)
            {
                webProxy = new WebProxyConfig()
                {
                    Address = proxyHttpClientOptions.WebProxyAddress?.ReadUri(),
                    BypassOnLocal = proxyHttpClientOptions.WebProxyBypassOnLocal,
                    UseDefaultCredentials = proxyHttpClientOptions.WebProxyUseDefaultCredentials
                };
            }
            else
            {
                webProxy = null;
            }
            return new Yarp.ReverseProxy.Configuration.HttpClientConfig
            {
                SslProtocols = sslProtocols,
                DangerousAcceptAnyServerCertificate = proxyHttpClientOptions.DangerousAcceptAnyServerCertificate,
                MaxConnectionsPerServer = proxyHttpClientOptions.MaxConnectionsPerServer.ReadInt32GTZero(),
                EnableMultipleHttp2Connections = proxyHttpClientOptions.EnableMultipleHttp2Connections,
                RequestHeaderEncoding = proxyHttpClientOptions.RequestHeaderEncoding?.ReadString(),
                WebProxy = webProxy,

            };
        }

        private static Yarp.ReverseProxy.Forwarder.ForwarderRequestConfig? CreateProxyRequestConfig(ForwarderRequest? requestProxyOptions)
        {
            if (requestProxyOptions is null)
            {
                return null;
            }

            return new Yarp.ReverseProxy.Forwarder.ForwarderRequestConfig
            {
                ActivityTimeout = requestProxyOptions.ActivityTimeout?.ReadTimeSpan(),
                Version = requestProxyOptions?.Version?.ReadVersion(),
                VersionPolicy = requestProxyOptions?.VersionPolicy?.ReadEnum<HttpVersionPolicy>(),
                AllowResponseBuffering = requestProxyOptions?.AllowResponseBuffering
            };
        }

        private static Yarp.ReverseProxy.Configuration.DestinationConfig CreateDestination(Destination destination)
        {
            return new Yarp.ReverseProxy.Configuration.DestinationConfig
            {
                Address = destination.Address,
                Health = destination.Health?.ReadString(),
                //Metadata = destination.Metadatas?.ReadStringDictionary(),
            };
        }
    }
}
