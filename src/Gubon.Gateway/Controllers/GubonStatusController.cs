using Gubon.Gateway.Store.FreeSql.Management;
using Gubon.Gateway.Store;
using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Gubon.Gateway.Authorization;
using Gubon.Gateway.Middleware.ContextStates;
using FreeRedis;
using Gubon.Gateway.Utils.Config;
using Newtonsoft.Json;
using Gubon.Gateway.IResult;
using Yarp.ReverseProxy.Model;

namespace Gubon.Gateway.Controllers
{
    [Route("__admin/api/[controller]")]
    [ApiController]
    [Authorize]
    public class GubonStatusController : ControllerBase
    {

        private readonly ILogger<GubonStatusController> _logger;
        private readonly IClusterManagement _clusterManagement;
        private readonly IProxyRouteManagement _proxyRouteManagement;
        private readonly IFreeSql _ifreeSql;
        private readonly IConfiguration _configuration;
        private readonly IProxyStateLookup _proxyStateLookup;
        private readonly IRedisClient _redisClient;
        private readonly GubonSettings _gubonsetting;

        public GubonStatusController(ILogger<GubonStatusController> logger, IClusterManagement clusterManagement, IProxyRouteManagement proxyRouteManagement
            , IConfiguration configuration, IFreeSql ifreeSql, IProxyStateLookup proxyStateLookup, GubonSettings gubonSettings, IRedisClient redisClient)
        {
            _logger = logger;
            _configuration = configuration;
            _ifreeSql = ifreeSql;
            _clusterManagement = clusterManagement;
            _proxyRouteManagement = proxyRouteManagement;
            _proxyStateLookup = proxyStateLookup;
            _redisClient = redisClient;
            _gubonsetting = gubonSettings; 
        }

        #region ServiceList 网关服务节点列表
        [HttpGet("serviceList")]
        public JsonResult ServiceList(string? servicename, string? order, string? sort, int from = 0, int limit = 20)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                ret.data = GetServiceListFromRedis();
            }
            else
            {
                ret.data = new List<GateWayInfo>
                {
                    GubonInfo.Instance
                };

            }
            return new JsonResult(ret);
        }
        [HttpPost("serviceList")]
        public async Task<JsonResult>  ServiceListClear(string servicename)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                var prefix = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{servicename}";
                await _redisClient.DelAsync(
                  $"{prefix}:{RedisKey.GubonCounterTotal}",
                  $"{prefix}:{RedisKey.GubonCounterRoutes}",
                  $"{prefix}:{RedisKey.GubonCounterRequests}",
                  $"{prefix}:{RedisKey.GubonCounterDestinations}");
            }
            else
            {
                StaticDestinationsStates.current.counter_all.Reset();
                StaticDestinationsStates.current.counter_2xx.Reset();
                StaticDestinationsStates.current.counter_3xx.Reset();
                StaticDestinationsStates.current.counter_4xx.Reset();
                StaticDestinationsStates.current.counter_5xx.Reset();
                StaticDestinationsStates.current._destinations.Clear();
                StaticDestinationsStates.current._request.Clear();
                StaticDestinationsStates.current._routes.Clear();
            }
            return new JsonResult(ret);
        }
        [HttpDelete("serviceList")]
        public async Task<JsonResult> ServiceListDelete(string? servicename)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                var prefix = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{servicename}";
                await _redisClient.DelAsync(
              $"{prefix}:{RedisKey.GubonCounterTotal}",
              $"{prefix}:{RedisKey.GubonCounterRoutes}",
              $"{prefix}:{RedisKey.GubonCounterRequests}",
              $"{prefix}:{RedisKey.GubonCounterDestinations}");
                var serviceKey = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayServices}";
                await _redisClient.HDelAsync(serviceKey, servicename);
            }
            return new JsonResult(ret);
        }
        #endregion

        #region RoutesCounter 路由请求统计
        [HttpGet("routeCounter")]
        public JsonResult RoutesCounter(string? servicename,string? order, string? sort, int from = 0, int limit = 999)
        {
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                return RoutesCounterRedis(order, sort, from, limit, servicename);
            }
            else
            {
                return RoutesCounterLocal(order, sort, from, limit);
                
            }
        }
        private JsonResult RoutesCounterLocal(string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var state_routes = new List<States>();
            var routes = StaticDestinationsStates.current._routes.ToArray();
            foreach (var r in routes)
            {
                state_routes.Add(new States
                {
                    Name = r.Key,
                    Counter2xx = r.Value.AtomicCounter2xx.Value,
                    Counter4xx = r.Value.AtomicCounter4xx.Value,
                    Counter5xx = r.Value.AtomicCounter5xx.Value,
                    Counter400 = r.Value.AtomicCounter400.Value,
                    Counter401 = r.Value.AtomicCounter401.Value,
                    Counter403 = r.Value.AtomicCounter403.Value,
                    Counter405 = r.Value.AtomicCounter405.Value,
                    Counter500 = r.Value.AtomicCounter500.Value,
                    Counter502 = r.Value.AtomicCounter502.Value,
                    Counter503 = r.Value.AtomicCounter503.Value,
                    Counter504 = r.Value.AtomicCounter504.Value,
                });
            }

            var state_routes_page = CounterSortedPage(state_routes, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = StaticDestinationsStates.current.StartDateTime;
            counter.state_all = StaticDestinationsStates.current.counter_all.Value;
            counter.state_2xx = StaticDestinationsStates.current.counter_2xx.Value;
            counter.state_4xx = StaticDestinationsStates.current.counter_4xx.Value;
            counter.state_5xx = StaticDestinationsStates.current.counter_5xx.Value;
            counter.routes_states = state_routes_page;
            ret.data = counter;
            ret.total = routes.LongLength;
            return new JsonResult(ret);
        }
        private JsonResult RoutesCounterRedis(string? order, string? sort, int from, int limit,string? servicename)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var startDateTime = DateTime.Now;
            var totalCounters = new Dictionary<string, long>();
            var totalRoutes = new Dictionary<string, long>();
            if (string.IsNullOrEmpty(servicename))
            {
                //获取网关节点列表
                var servicesList = GetServiceListFromRedis(); ;
                foreach (var gatewayInfo in servicesList)
                {
                    var gatewayPrefix = $"{gatewayInfo.ClusterName}:{gatewayInfo.ServiceName}";
                    startDateTime = DateTime.Compare(gatewayInfo.StartTime, startDateTime) < 0 ? gatewayInfo.StartTime : startDateTime;
                    //获取节点Total数据
                    var gatewayTotalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                    //合并到总Total数据
                    foreach(var t in gatewayTotalCounters)
                    {
                        if (totalCounters.ContainsKey(t.Key))
                        {
                            totalCounters[t.Key] = totalCounters[t.Key] + t.Value;
                        }
                        else
                        {
                            totalCounters.Add(t.Key, t.Value);
                        }
                    }
                    //获取节点 Routes
                    var gatewayRoutes = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterRoutes}").Result;
                    //合并到总 Routes
                    foreach (var t in gatewayRoutes)
                    {
                        if (totalRoutes.ContainsKey(t.Key))
                        {
                            totalRoutes[t.Key] = totalRoutes[t.Key] + t.Value;
                        }
                        else
                        {
                            totalRoutes.Add(t.Key, t.Value);
                        }
                    }
                }
            }
            else
            {
                var gatewayPrefix = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{servicename}";
                //获取网关节点列表
                var servicesList = GetServiceListFromRedis(); ;
                var serive = servicesList.Where(s => s.ServiceName == servicename).First();
                startDateTime = serive.StartTime;

                totalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                totalRoutes = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterRoutes}").Result;
            }

            //total
           // var totalCounters = _redisClient.HGetAllAsync<long>("gubon.counter.total").Result;

            // routes
            var state_routes = new Dictionary<string, States>();
          //  var routes = _redisClient.HGetAllAsync<long>("gubon.counter.routes").Result;
            GetRedisStateCounter(state_routes, totalRoutes);

            var state_routes_list = state_routes.Values.ToList();
            var state_routes_page = CounterSortedPage(state_routes_list, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = startDateTime;
            totalCounters?.TryGetValue("all", out counter.state_all);
            totalCounters?.TryGetValue("2xx", out counter.state_2xx);
            totalCounters?.TryGetValue("4xx", out counter.state_4xx);
            totalCounters?.TryGetValue("5xx", out counter.state_5xx);
            counter.routes_states = state_routes_page;
            ret.total = state_routes_list.Count;
            ret.data = counter;
            return new JsonResult(ret);
        }
        #endregion

        #region DestinationsCounter 目的地请求统计
        [HttpGet("destCounter")]
        public JsonResult DestinationsCounter(string? servicename, string? order, string? sort, int from = 0, int limit = 999)
        {
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                return DestinationsCounterRedis(servicename, order, sort, from, limit); 
            }
            else
            {
                return DestinationsCounterLocal(order, sort, from, limit);
            }
        }

        private JsonResult DestinationsCounterLocal(string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var state_destinations = new List<States>();
            var destinations = StaticDestinationsStates.current._destinations.ToArray();
            foreach (var r in destinations)
            {
                state_destinations.Add(new States
                {
                    Name = r.Key,
                    Counter2xx = r.Value.AtomicCounter2xx.Value,
                    Counter4xx = r.Value.AtomicCounter4xx.Value,
                    Counter5xx = r.Value.AtomicCounter5xx.Value,
                    Counter400 = r.Value.AtomicCounter400.Value,
                    Counter401 = r.Value.AtomicCounter401.Value,
                    Counter403 = r.Value.AtomicCounter403.Value,
                    Counter405 = r.Value.AtomicCounter405.Value,
                    Counter500 = r.Value.AtomicCounter500.Value,
                    Counter502 = r.Value.AtomicCounter502.Value,
                    Counter503 = r.Value.AtomicCounter503.Value,
                    Counter504 = r.Value.AtomicCounter504.Value,
                });
            }
            var state_destinations_page = CounterSortedPage(state_destinations, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = StaticDestinationsStates.current.StartDateTime;
            counter.state_all = StaticDestinationsStates.current.counter_all.Value;
            counter.state_2xx = StaticDestinationsStates.current.counter_2xx.Value;
            counter.state_4xx = StaticDestinationsStates.current.counter_4xx.Value;
            counter.state_5xx = StaticDestinationsStates.current.counter_5xx.Value;
            counter.destinations_states = state_destinations_page;
            ret.data = counter;
            ret.total = destinations.LongLength;
            return new JsonResult(ret);
        }
        private JsonResult DestinationsCounterRedis(string? servicename, string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var startDateTime = DateTime.Now;
            var totalCounters = new Dictionary<string, long>();
            var totalDestinations = new Dictionary<string, long>();
            if (string.IsNullOrEmpty(servicename))
            {
                //获取网关节点列表
                var servicesList =  GetServiceListFromRedis(); ;
                foreach (var gatewayInfo in servicesList)
                {
                    var gatewayPrefix = $"{gatewayInfo.ClusterName}:{gatewayInfo.ServiceName}";
                    startDateTime = DateTime.Compare(gatewayInfo.StartTime, startDateTime) < 0 ? gatewayInfo.StartTime : startDateTime;
                    //获取节点Total数据
                    var gatewayTotalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                    //合并到总Total数据
                    foreach (var t in gatewayTotalCounters)
                    {
                        if (totalCounters.ContainsKey(t.Key))
                        {
                            totalCounters[t.Key] = totalCounters[t.Key] + t.Value;
                        }
                        else
                        {
                            totalCounters.Add(t.Key, t.Value);
                        }
                    }
                    //获取节点 Destinations
                    var gatewayDests = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterDestinations}").Result;
                    //合并到总 Destinations
                    foreach (var t in gatewayDests)
                    {
                        if (totalDestinations.ContainsKey(t.Key))
                        {
                            totalDestinations[t.Key] = totalDestinations[t.Key] + t.Value;
                        }
                        else
                        {
                            totalDestinations.Add(t.Key, t.Value);
                        }
                    }
                }
            }
            else
            {
                var gatewayPrefix = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{servicename}";
                //获取网关节点列表
                var servicesList = GetServiceListFromRedis(); ;
                var serive = servicesList.Where(s => s.ServiceName== servicename).First();
                startDateTime = serive.StartTime;
                totalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                totalDestinations = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterDestinations}").Result;
            }
           

            //destinations
            var state_destinations = new Dictionary<string, States>();
          //  var destinations = _redisClient.HGetAllAsync<long>("gubon.counter.destinations").Result;
            GetRedisStateCounter(state_destinations, totalDestinations);

            var state_destinations_list = state_destinations.Values.ToList();

            var state_destinations_page = CounterSortedPage(state_destinations_list, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = startDateTime;
            totalCounters?.TryGetValue("all", out counter.state_all);
            totalCounters?.TryGetValue("2xx", out counter.state_2xx);
            totalCounters?.TryGetValue("4xx", out counter.state_4xx);
            totalCounters?.TryGetValue("5xx", out counter.state_5xx);
            counter.destinations_states = state_destinations_page;
            ret.total = state_destinations_list.Count;
            ret.data = counter;
            return new JsonResult(ret);

        }
        #endregion

        #region RequestCounter 请求地址统计
        [HttpGet("requestCounter")]
        public JsonResult RequestCounter(string? servicename, string? order, string? sort, int from = 0, int limit = 15)
        {
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                return RequestCounterRedis(servicename, order, sort, from, limit);
            }
            else
            {
                return RequestCounterLocal(order, sort, from, limit);
            }
        }

        private JsonResult RequestCounterLocal(string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var state_requests_list = new List<States>();
            var requests = StaticDestinationsStates.current._request.ToArray();
            foreach (var r in requests)
            {
                state_requests_list.Add(new States
                {
                    Name = r.Key,
                    Counter2xx = r.Value.AtomicCounter2xx.Value,
                    Counter4xx = r.Value.AtomicCounter4xx.Value,
                    Counter5xx = r.Value.AtomicCounter5xx.Value,
                    Counter400 = r.Value.AtomicCounter400.Value,
                    Counter401 = r.Value.AtomicCounter401.Value,
                    Counter403 = r.Value.AtomicCounter403.Value,
                    Counter405 = r.Value.AtomicCounter405.Value,
                    Counter500 = r.Value.AtomicCounter500.Value,
                    Counter502 = r.Value.AtomicCounter502.Value,
                    Counter503 = r.Value.AtomicCounter503.Value,
                    Counter504 = r.Value.AtomicCounter504.Value,


                });
            }

            var state_requests_page = CounterSortedPage(state_requests_list, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = StaticDestinationsStates.current.StartDateTime;
            counter.request_states = state_requests_page;
            ret.total = requests.LongLength;
            ret.data = counter;
            return new JsonResult(ret);
        }

        private JsonResult RequestCounterRedis(string? servicename, string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var startDateTime = DateTime.Now;
            var totalCounters = new Dictionary<string, long>();
            var totalRequests = new Dictionary<string, long>();
            if (string.IsNullOrEmpty(servicename))
            {
                //获取网关节点列表
                var servicesList = GetServiceListFromRedis(); ;
                foreach (var gatewayInfo in servicesList)
                {

                    var gatewayPrefix = $"{gatewayInfo.ClusterName}:{gatewayInfo.ServiceName}";
                    startDateTime = DateTime.Compare(gatewayInfo.StartTime, startDateTime) < 0 ? gatewayInfo.StartTime : startDateTime;
                    //获取节点Total数据
                    var gatewayTotalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                    //合并到总Total数据
                    foreach (var t in gatewayTotalCounters)
                    {
                        if (totalCounters.ContainsKey(t.Key))
                        {
                            totalCounters[t.Key] = totalCounters[t.Key] + t.Value;
                        }
                        else
                        {
                            totalCounters.Add(t.Key, t.Value);
                        }
                    }
                    //获取节点 Destinations
                    var gatewayRequests = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterRequests}").Result;
                    //合并到总 Destinations
                    foreach (var t in gatewayRequests)
                    {
                        if (totalRequests.ContainsKey(t.Key))
                        {
                            totalRequests[t.Key] = totalRequests[t.Key] + t.Value;
                        }
                        else
                        {
                            totalRequests.Add(t.Key, t.Value);
                        }
                    }
                }
            }
            else
            {
                var gatewayPrefix = $"{_gubonsetting.GubonHttpCounter.ClusterName}:{servicename}";
                //获取网关节点列表
                var servicesList = GetServiceListFromRedis(); ;
                var serive = servicesList.Where(s => s.ServiceName == servicename).First();
                startDateTime = serive.StartTime;
                totalCounters = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterTotal}").Result;
                totalRequests = _redisClient.HGetAllAsync<long>($"{gatewayPrefix}:{RedisKey.GubonCounterRequests}").Result;
            }


            // request
            var state_requests = new Dictionary<string, States>();
            GetRedisStateCounter(state_requests, totalRequests);
            var state_requests_list = state_requests.Values.ToList();

            var state_requests_page = CounterSortedPage(state_requests_list, sort, order, from, limit);

            var counter = new GubonStateCounterDto();
            counter.StartDateTime = startDateTime;
            counter.request_states = state_requests_page;
            ret.total = state_requests_list.Count;
            ret.data = counter;
            return new JsonResult(ret);
        }
        #endregion

        #region 获取异常日志
        [HttpGet("errorLogsList")]
        public async Task<JsonResult> ErrorLogs(string? method, string scheme, string? requestPath, string? host,string? ip, int? statusCode, DateTime? beginTime, DateTime? endTime,
            string? order, string? sort, int from = 0, int limit = 15)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var bTime = beginTime ?? DateTime.MinValue;
            var eTime = endTime ?? DateTime.MinValue;

            var list = await _ifreeSql.GetRepository<Logs>()
                .WhereIf(!string.IsNullOrWhiteSpace(requestPath), x => x.RequestPath.Contains(requestPath))
                .WhereIf(!string.IsNullOrWhiteSpace(method), x => x.Method == method)
                .WhereIf(!string.IsNullOrWhiteSpace(scheme), x => x.Scheme == scheme)
                .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
                .WhereIf(!string.IsNullOrWhiteSpace(ip), x => x.Ip == ip)
                .WhereIf(statusCode > 0, x => x.StatusCode == statusCode)
                .WhereIf(bTime > DateTime.MinValue, x => x.ResponseTime >= bTime)
                .WhereIf(eTime > DateTime.MinValue, x => x.ResponseTime <= eTime)
                .OrderBy(!string.IsNullOrWhiteSpace(order) && order == "desc", sort + " desc")
                .OrderBy(!string.IsNullOrWhiteSpace(order) && order != "desc", sort)
                .OrderByDescending(string.IsNullOrWhiteSpace(order), x => x.ResponseTime)
                .Skip(from).Take(limit).ToListAsync();

            var total = await _ifreeSql.GetRepository<Logs>()
                .WhereIf(!string.IsNullOrWhiteSpace(requestPath), x => x.RequestPath.Contains(requestPath))
                .WhereIf(!string.IsNullOrWhiteSpace(method), x => x.Method == method)
                .WhereIf(!string.IsNullOrWhiteSpace(scheme), x => x.Scheme == scheme)
                .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
                .WhereIf(!string.IsNullOrWhiteSpace(ip), x => x.Ip == ip)
                .WhereIf(statusCode > 0, x => x.StatusCode == statusCode)
                .WhereIf(bTime > DateTime.MinValue, x => x.ResponseTime >= bTime)
                .WhereIf(eTime > DateTime.MinValue, x => x.ResponseTime <= eTime)
                .CountAsync();
            ret.code = 0;
            ret.data = list;
            ret.total = total;
            return new JsonResult(ret);
        }

        [HttpGet("errorLogsInfo")]
        public async Task<JsonResult> ErrorLogsInfo(int id)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var list = await _ifreeSql.GetRepository<Logs>()
                .Where(x => x.Id == id).FirstAsync();

            ret.code = 0;
            ret.data = list;
            ret.total = 1;
            return new JsonResult(ret);
        }
        
        /// <summary>
        /// 删除选中的logs
        /// </summary>
        /// <param name="ids">选中的id</param>
        /// <returns></returns>
        [HttpPost("errorLogsDelete")]
        public async Task<JsonResult> ErrorLogsDelete(int[] ids)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var cnt = await _ifreeSql.GetRepository<Logs>().DeleteAsync(x=>ids.Contains(x.Id));
            ret.code = 0;
            ret.data = cnt;
            ret.total =cnt ;
            return new JsonResult(ret);
        }

        /// <summary>
        /// 删除所有的Logs
        /// </summary>
        /// <returns></returns>
        [HttpDelete("errorLogsDeleteAll")]
        public async Task<JsonResult> ErrorLogsDeleteAll()
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var cnt =await  _ifreeSql.Delete<Logs>().Where("1=1").ExecuteAffrowsAsync();
            ret.code = 0;
            ret.data = cnt;
            ret.total = cnt;
            return new JsonResult(ret);
        }

        #endregion

        #region 获取网关生效的信息列表

        [HttpGet("gatewayActiveClusterList")]
        public async Task<JsonResult> GatewayActiveClusterList(string? clusterId, string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            //搜索
            if (!string.IsNullOrWhiteSpace(clusterId))
            {
                await Task.Run(() =>
                {
                    var clusters = _proxyStateLookup.GetClusters().Where(x => x.ClusterId.Contains(clusterId)).Skip(from).Take(limit).ToList();
                    if (clusters != null && clusters.Any())
                    {
                        ret.data = clusters;
                        ret.total = _proxyStateLookup.GetClusters().Where(x => x.ClusterId.Contains(clusterId)).Count();
                    }
                });
                return new JsonResult(ret);
            }

            //分页Route
            await Task.Run(() =>
            {
                var clusterList = _proxyStateLookup.GetClusters().Skip(from).Take(limit).ToList();
                var total = _proxyStateLookup.GetClusters().Count();
                if (clusterList != null && clusterList.Any())
                {
                    ret.data = clusterList;
                    ret.total = total;
                }
            });
            return new JsonResult(ret);
        }

        /// <summary>
        /// 网关生效路由分页列表
        /// </summary>
        /// <param name="routeId">要查询的路由ID</param>
        /// <param name="order"></param>
        /// <param name="sort"></param>
        /// <param name="from"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("gatewayActiveRouteList")]
        public async Task<JsonResult> GatewayActiveRouteList(string? routeId, string? order, string? sort, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            //搜索单个Route
            if (!string.IsNullOrWhiteSpace(routeId))
            {
                await Task.Run(() =>
                {
                    var routes = _proxyStateLookup.GetRoutes().Select(x => x.Config).Where(x => x.RouteId.Contains(routeId))
                    .OrderBy(x => x.Order).Skip(from).Take(limit).ToList();
                    if (routes != null && routes.Any())
                    {
                        ret.data = routes;
                        ret.total = _proxyStateLookup.GetRoutes().Select(x => x.Config).Where(x => x.RouteId.Contains(routeId)).Count();
                    }
                });
                return new JsonResult(ret);
            }

            //分页Route
            await Task.Run(() =>
            {
                var routeList = _proxyStateLookup.GetRoutes().Select(x => x.Config).Skip(from).Take(limit).OrderBy(x => x.Order).ToList();
                var total = _proxyStateLookup.GetRoutes().Select(x => x.Config).Count();
                if (routeList != null && routeList.Any())
                {
                    ret.data = routeList;
                    ret.total = total;
                }
            });
            return new JsonResult(ret);
        }
        /// <summary>
        /// 获取生效路由的信息
        /// </summary>
        /// <param name="routeId">路由ID</param>
        /// <returns></returns> 
        [HttpGet("gatewayActiveRouteInfo")]
        public async Task<JsonResult> GatewayActiveRouteInfo(string routeId)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            //搜索单个Route
            await Task.Run(() =>
            {
                if( _proxyStateLookup.TryGetRoute(routeId,out var route))
                {
                    ret.data = route.Config;
                    ret.total = 1;
                }
            });
            return new JsonResult(ret);

        }
        /// <summary>
        /// 获取当前生效的目的地列表
        /// </summary>
        /// <param name="destId"></param>
        /// <param name="from"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("gatewayActiveDestList")]
        [AllowAnonymous]
        public JsonResult GatewayActiveDestList(string? clusterId,string? destId, string? host, int from, int limit)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            List<ClusterState> clusters;
            var total = 0;
            if (!string.IsNullOrWhiteSpace(clusterId))
            {
                clusters = _proxyStateLookup.GetClusters().Where(x=>x.ClusterId.Contains(clusterId)).Skip(from).Take(limit).ToList();
                total = _proxyStateLookup.GetClusters().Where(x => x.ClusterId.Contains(clusterId)).Count();
            }
            else
            {
                clusters = _proxyStateLookup.GetClusters().Skip(from).Take(limit).ToList();
                total = _proxyStateLookup.GetClusters().Count();
            }

           
            if (clusters is null)
            {
                return new JsonResult(ret);
            }
            var newClusters = new List<ClusterStateDto>();
            foreach (var c in clusters)
            {
                var newc = new ClusterStateDto { ClusterId = c.ClusterId };
                // all
                var newAll = new List<DestinationStateDto>();
                var newAvailable = new List<DestinationStateDto>();
                var AllDestinations = c.DestinationsState.AllDestinations.ToList();
                if (!string.IsNullOrWhiteSpace(destId)) { 
                    AllDestinations = AllDestinations.Where(x => x.DestinationId.Contains(destId)).ToList();
                    if(AllDestinations.Count ==0) { continue; }
                }
                if (!string.IsNullOrWhiteSpace(host))
                {
                    AllDestinations = AllDestinations.Where(x => x.Model.Config.Address.Contains(host)).ToList();
                    if (AllDestinations.Count == 0) { continue; }
                }

                foreach (var c2 in AllDestinations)
                {
                    newAll.Add(new DestinationStateDto
                    {
                        DestinationId = c2.DestinationId,
                        ConcurrentRequestCount = c2.ConcurrentRequestCount,
                        Address = c2.Model.Config.Address,
                        Health = c2.Health,
                        Available = false
                    });
                }

                foreach (var d in c.DestinationsState.AvailableDestinations)
                {
                    newAvailable.Add(new DestinationStateDto
                    {
                        DestinationId = d.DestinationId,
                        ConcurrentRequestCount = d.ConcurrentRequestCount,
                        Address = d.Model.Config.Address,
                        Health = d.Health,
                        Available = true
                    });
                }

                var AvailableDestIds = newAvailable.Select(d => d.DestinationId);
                foreach (var d in newAll)
                {
                    d.Available = AvailableDestIds.Contains(d.DestinationId);
                }

                newc.AllDestinations = newAll;
                //newc.AvailableDestinations = newAvailable;
                newClusters.Add(newc);
            }
            ret.total = total;
            ret.data = newClusters;

            return new JsonResult(ret);
        }

        #endregion

        #region Private Functions 私有方法
        private void GetRedisStateCounter(Dictionary<string, States> dest, Dictionary<string, long> source)
        {
            foreach (var r in source)
            {
                var status_code = r.Key.Substring(0, 3);
                var keyString = r.Key.Substring(4);

                RedisStateCounterUpdate(dest, keyString, r.Value, status_code);
            }
        }

        private void RedisStateCounterUpdate(Dictionary<string, States> dest,string key ,long val,string code)
        {
            States state;

            if (dest.ContainsKey(key))
            {
               var  temstate = dest.GetValueOrDefault(key);
                if (temstate == null)
                    state = new States();
                else
                    state = temstate;               
            }
            else
            {
                state =  new States
                {
                    Name = key
                };
            }
            switch (code)
            {
                case "400":
                    state.Counter400 = val;
                    break;
                case "401":
                    state.Counter401 = val;
                    break;
                case "403":
                    state.Counter403 = val;
                    break;
                case "405":
                    state.Counter405 = val;
                    break;
                case "500":
                    state.Counter500 = val;
                    break;
                case "502":
                    state.Counter502 = val;
                    break;
                case "503":
                    state.Counter503 = val;
                    break;
                case "504":
                    state.Counter504 = val;
                    break;
                case "2xx":
                    state.Counter2xx = val;
                    break;
                case "4xx":
                    state.Counter4xx = val;
                    break;
                case "5xx":
                    state.Counter5xx = val;
                    break;
            }
            dest[key] = state;
        }

        private List<States> CounterSortedPage(List<States> data, string? sort, string? order, int from, int limit)
        {
            if (!string.IsNullOrWhiteSpace(sort) && !string.IsNullOrWhiteSpace(order))
            {
                if (order.ToLower() == "desc")
                {
                    switch (sort)
                    {
                        case "counter400":
                            data = data.OrderByDescending(p => p.Counter400).Skip(from).Take(limit).ToList();
                            break;
                        case "counter401":
                            data = data.OrderByDescending(p => p.Counter401).Skip(from).Take(limit).ToList();
                            break;
                        case "counter403":
                            data = data.OrderByDescending(p => p.Counter403).Skip(from).Take(limit).ToList();
                            break;
                        case "counter405":
                            data = data.OrderByDescending(p => p.Counter405).Skip(from).Take(limit).ToList();
                            break;
                        case "counter500":
                            data = data.OrderByDescending(p => p.Counter500).Skip(from).Take(limit).ToList();
                            break;
                        case "counter502":
                            data = data.OrderByDescending(p => p.Counter502).Skip(from).Take(limit).ToList();
                            break;
                        case "counter503":
                            data = data.OrderByDescending(p => p.Counter503).Skip(from).Take(limit).ToList();
                            break;
                        case "counter504":
                            data = data.OrderByDescending(p => p.Counter504).Skip(from).Take(limit).ToList();
                            break;
                        case "counter2xx":
                            data = data.OrderByDescending(p => p.Counter2xx).Skip(from).Take(limit).ToList();
                            break;
                        case "counter4xx":
                            data = data.OrderByDescending(p => p.Counter4xx).Skip(from).Take(limit).ToList();
                            break;
                        case "counter5xx":
                            data = data.OrderByDescending(p => p.Counter5xx).Skip(from).Take(limit).ToList();
                            break;
                        default:
                            data = data.OrderByDescending(p => p.Name).Skip(from).Take(limit).ToList();
                            break; ;
                    }
                }
                else
                {
                    switch (sort)
                    {
                        case "counter400":
                            data = data.OrderBy(p => p.Counter400).Skip(from).Take(limit).ToList();
                            break;
                        case "counter401":
                            data = data.OrderBy(p => p.Counter401).Skip(from).Take(limit).ToList();
                            break;
                        case "counter403":
                            data = data.OrderBy(p => p.Counter403).Skip(from).Take(limit).ToList();
                            break;
                        case "counter405":
                            data = data.OrderBy(p => p.Counter405).Skip(from).Take(limit).ToList();
                            break;
                        case "counter500":
                            data = data.OrderBy(p => p.Counter500).Skip(from).Take(limit).ToList();
                            break;
                        case "counter502":
                            data = data.OrderBy(p => p.Counter502).Skip(from).Take(limit).ToList();
                            break;
                        case "counter503":
                            data = data.OrderBy(p => p.Counter503).Skip(from).Take(limit).ToList();
                            break;
                        case "counter504":
                            data = data.OrderBy(p => p.Counter504).Skip(from).Take(limit).ToList();
                            break;
                        case "counter2xx":
                            data = data.OrderBy(p => p.Counter2xx).Skip(from).Take(limit).ToList();
                            break;
                        case "counter4xx":
                            data = data.OrderBy(p => p.Counter4xx).Skip(from).Take(limit).ToList();
                            break;
                        case "counter5xx":
                            data = data.OrderBy(p => p.Counter5xx).Skip(from).Take(limit).ToList();
                            break;
                        default:
                            data = data.OrderBy(p => p.Name).Skip(from).Take(limit).ToList();
                            break; ;
                    }
                }
            }
            else
            {
                data = data.OrderByDescending(p => p.Counter2xx).Skip(from).Take(limit).ToList();
            }
            return data;
        }

        private List<GateWayInfo> GetServiceListFromRedis()
        {
            var GatewayInfos = new List<GateWayInfo>();
            if (_gubonsetting.GubonHttpCounter.StoreInRedis)
            {
                var data = _redisClient.HGetAllAsync<string>($"{_gubonsetting.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayServices}").Result.Values.ToList();
                foreach (var s in data)
                {
                    var g = JsonConvert.DeserializeObject<GateWayInfo>(s);
                    if (g != null)
                    {
                        GatewayInfos.Add(g);
                    }
                }
            }
            return GatewayInfos;
        }
        #endregion

    }
}