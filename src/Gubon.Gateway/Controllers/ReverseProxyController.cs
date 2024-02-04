using FreeSql;
using Gubon.Gateway.Authorization;
using Gubon.Gateway.IResult;
using Gubon.Gateway.Store;
using Gubon.Gateway.Store.FreeSql;
using Gubon.Gateway.Store.FreeSql.Management;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Validate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Metrics;
using System.Text;
using Yarp.ReverseProxy;

namespace Gubon.Gateway.Controllers
{
    [Route("__admin/api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReverseProxyController : ControllerBase
    {
        private readonly ILogger<ReverseProxyController> _logger;
        private readonly IClusterManagement _clusterManagement;
        private readonly IProxyRouteManagement _proxyRouteManagement;
        private readonly IFreeSql _ifreeSql;
        private readonly IConfiguration _configuration;
        private readonly IProxyStateLookup _proxyStateLookup;
        private readonly ClusterValidator _clusterValidator;
        private readonly ProxyRouteValidator _proxyRouteValidator;
        private readonly IMemoryCache _memoryCache;
        public ReverseProxyController(ILogger<ReverseProxyController> logger, IClusterManagement clusterManagement, IProxyRouteManagement proxyRouteManagement
            , IConfiguration configuration,IFreeSql ifreeSql , IProxyStateLookup proxyStateLookup, ClusterValidator clusterValidator, 
            ProxyRouteValidator proxyRouteValidator,IMemoryCache memoryCache)
        {
            _logger = logger;
            _configuration = configuration;
            _ifreeSql = ifreeSql;
            _clusterManagement = clusterManagement;
            _proxyRouteManagement = proxyRouteManagement;
            _proxyStateLookup = proxyStateLookup;
            _clusterValidator = clusterValidator;
            _proxyRouteValidator = proxyRouteValidator;
            _memoryCache = memoryCache;
        }

   

        //[HttpGet("Create")]
        //public async Task<ActionResult> Create()
        //{
        //    SyncStructure.Sync(_ifreeSql);
        //    //            Type[] types = typeof(Gubon.Gateway.Store.SqlSugar.Models.Cluster).Assembly.GetTypes()
        //    //.Where(it => it.FullName.Contains("Gubon.Gateway.Store.SqlSugar.Models."))//命名空间过滤，当然你也可以写其他条件过滤
        //    //.Where(it=>it.IsClass)
        //    //.ToArray();
        //    //            _sqlSugarClient.CodeFirst.SetStringDefaultLength(200).InitTables(types);
        //    return Ok(new { Data = true });
        //}


        #region Cluster

        [HttpGet("ClusterInfo")]
        public async Task<ActionResult> GetClusterInfo(int id)
        {
            var cluster = await _clusterManagement.Find(id);
            /// var cluster = clusters[0];

            // var total = await _clusterManagement.GetAll().CountAsync();
            var ret = new JResult() { data = cluster, total = 1, status = 1, error = string.Empty };

            return Ok(ret);
        }


        [HttpGet("ClusterPage")]
        public async Task<ActionResult> GetClusterPage(int from = 1, int limit = 10)
        {
            var clusters = await _clusterManagement.GetAll()
                .IncludeMany(c => c.Destinations)
                //.Include(c => c.HealthCheckConfig.Active)
                //.Include(c => c.HealthCheckConfig.Passive)
                .Skip(from)
                .Take(limit)
                .ToListAsync(true);
           
            var total = await _clusterManagement.GetAll().CountAsync();
            var ret  = new JResult() { data= clusters ,total=total,status=1,error=string.Empty};
            
            return Ok(ret);
        }
        [HttpGet("AllCluster")]
        public async Task<ActionResult> GetAllCluster()
        {
            var clusters = await _ifreeSql.Select<Cluster>().ToListAsync(c => new { ClusterId = c.Id, ClusterName = c.ClusterName });
 
            var ret = new JResult() { data = clusters, total = clusters.Count, status = 1, error = string.Empty };

            return Ok(ret);
        }
        [HttpPost("Cluster")]
        public async Task<ActionResult> AddCluster(Cluster cluster)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = "error" };
            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code= 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }
            //验证
            var retsult =  _clusterValidator.Validate(cluster);
            if (!retsult.IsValid)
            {
                ret.data = retsult.Errors.Select(e=>e.ErrorMessage + "->" + e.AttemptedValue?.ToString()).ToList();
                ret.code= 1;
                return Ok(ret);
            }
            //判断集群名称
            if (_clusterManagement.GetAll().Where(c => c.ClusterName == cluster.ClusterName).Any())
            {
                ret.code = 1;
                ret.error = $"集群[{cluster.ClusterName}]已存在";
                return Ok(ret);
            }

            // 判断目的地名称是否重复
            var errDest = new List<string>();
            var currDestName = cluster.Destinations.Select(dest => dest.DestName).ToList();
            var distinctCurrDestNameCount = currDestName.Distinct().Count();
            if (currDestName.Count() != distinctCurrDestNameCount)
            {
                ret.code = 1;
                ret.error = $"目的地名称不能相同";
                return Ok(ret);
            }
            var existsDestName = _ifreeSql.Select<Destination>().ToList(d => d.DestName);
            foreach (var dest in currDestName)
            {
                if (existsDestName.Contains(dest))
                {
                    errDest.Add(dest);
                }
            }
            if (errDest.Count() > 0)
            {
                ret.code = 1;
                ret.error = $"目的地[{string.Join(",", errDest)}]已存在";
                return Ok(ret);
            }

            //更新集群数据
            var res = await _clusterManagement.Create(cluster);
            return Ok(ret);
        }
        [HttpPut("Cluster")]
        public async Task<ActionResult> UpdateCluster(Cluster cluster)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code = 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }
            var retsult = _clusterValidator.Validate(cluster);
            if (!retsult.IsValid)
            {
                ret.data = retsult.Errors.Select(e => e.ErrorMessage + "->" + e.AttemptedValue?.ToString()).ToList();
                ret.code = 1;
                return Ok(ret);
            }

            if (_clusterManagement.GetAll().Where(c => c.ClusterName ==cluster.ClusterName && c.Id != cluster.Id).Any())
            {
                ret.code = 1;
                ret.error = $"集群[{cluster.ClusterName}]已存在";
                return Ok(ret);
            }


            // 判断目的地名称是否重复
            var errDest = new List<string>();
            var currDestName = cluster.Destinations.Select(dest => dest.DestName).ToList();
            var distinctCurrDestNameCount = currDestName.Distinct().Count();
            if(currDestName.Count()!= distinctCurrDestNameCount)
            {
                ret.code = 1;
                ret.error = $"目的地名称不能相同";
                return Ok(ret);
            }
            var existsDestName = _ifreeSql.Select<Destination>().Where(d=>d.ClusterId != cluster.Id).ToList(d=>d.DestName);
            foreach (var dest in currDestName) {
                if (existsDestName.Contains(dest))
                {
                    errDest.Add(dest);
                }
            }
            if(errDest.Count()> 0)
            {
                ret.code = 1;
                ret.error = $"目的地[{string.Join(",",errDest)}]已存在";
                return Ok(ret);
            }

            //更新集群数据
            var res = await _clusterManagement.Update(cluster);
            return Ok(ret);
            
        }
        [HttpDelete("Cluster")]
        public async Task<ActionResult> DeleteCluster(int id)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };

            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code = 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }

            var res = await _clusterManagement.Delete(id);

            return Ok(ret);

            //if (res)
            //    return Ok(new { Data = true });
            //else
            //    return Ok(new { Data = false });
        }
        #endregion

        #region ProxyRoute
        [HttpGet("ProxyRoute")]
        public async Task<ActionResult> GetProxyRoute(int id)
        {
            var routers = await _proxyRouteManagement.Find(id);
            var ret = new JResult() { data = routers, total = 1, status = 1, error = string.Empty };

            return Ok(ret);
 
        }
        [HttpGet("ProxyRoutePage")]
        public async Task<ActionResult> GetProxyRoutePage(int from = 1, int limit = 10)
        {
            var routers = await _proxyRouteManagement.GetAll()
                .Skip(from)
                .Take(limit)
                .OrderBy(c=>c.Order)
                .ToListAsync();
            var total = await _proxyRouteManagement.GetAll().CountAsync();

            var ret = new JResult() { data = routers, total = total, status = 1, error = string.Empty };
            return Ok(ret);
        }
        [HttpPost("ProxyRoute")]
        public async Task<ActionResult> AddProxyRoute(ProxyRoute proxyRoute)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code = 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }
            var retsult = _proxyRouteValidator.Validate(proxyRoute);
            if (!retsult.IsValid)
            {
                ret.data = retsult.Errors.Select(e => e.ErrorMessage + "->" + e.AttemptedValue?.ToString()).ToList();
                ret.code = 1;
                return Ok(ret);
            }


            if (_proxyRouteManagement.GetAll().Where(c => c.RouteName == proxyRoute.RouteName).Any())
            {
                ret.code = 1;
                ret.error = $"路由[{proxyRoute.RouteName}]已存在";
            }
            else
            {
                var res = await _proxyRouteManagement.Create(proxyRoute);
            }
          
            return Ok(ret);
        }
        [HttpPut("ProxyRoute")]
        public async Task<ActionResult> UpdateProxyRoute(ProxyRoute proxyRoute)
        {
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty }; 
            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code = 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }
            var retsult = _proxyRouteValidator.Validate(proxyRoute);
            if (!retsult.IsValid)
            {
                ret.data = retsult.Errors.Select(e => e.ErrorMessage+ "->"+e.AttemptedValue?.ToString()).ToList();
                ret.code = 1;
                return Ok(ret);
            }

            if (_proxyRouteManagement.GetAll().Where(c => c.RouteName == proxyRoute.RouteName && c.Id != proxyRoute.Id).Any())
            {
                ret.code = 1;
                ret.error = $"路由[{proxyRoute.RouteName}]已存在";
            }
            else
            {
                var res = await _proxyRouteManagement.Update(proxyRoute);
            }
            return Ok(ret);
        }
        [HttpDelete("ProxyRoute")]
        public async Task<ActionResult> DeleteProxyRoute(int id)
        {
          
            var ret = new JResult() { data = null, total = 0, status = 1, error = string.Empty };
            var user = HttpContext.Items["User"] as User;
            if (user != null && user.Role != "Admin")
            {
                ret.code = 1;
                ret.error = "无操作权限";
                return Ok(ret);
            }
            var res = await _proxyRouteManagement.Delete(id);
            return Ok(ret);
        }
        #endregion

      
    }
}
