using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Gubon.Gateway.Utils.Config;
using FreeRedis;

namespace Gubon.Gateway.Store.FreeSql.Management
{
    public class ProxyRouteManagement : IProxyRouteManagement
    {
        private readonly ILogger<ProxyRouteManagement> _logger;
        private IFreeSql DbContext;
        private readonly IReverseProxyStore _reverseProxyStore;
        private readonly IRedisClient _redisClient;
        private readonly GubonSettings _gubonSettings;

        public ProxyRouteManagement(IFreeSql dbContext, IReverseProxyStore reverseProxyStore, ILogger<ProxyRouteManagement> logger,
            IRedisClient redisClient, GubonSettings gubonSetting)
        {
            DbContext = dbContext;
            _reverseProxyStore = reverseProxyStore;
            _logger = logger; 
            _redisClient = redisClient;
            _gubonSettings = gubonSetting;
        }

        public async Task<bool> Create(ProxyRoute proxyRoute)
        {
            proxyRoute.Match.Methods = proxyRoute.Match.Methods.Trim(',');

            var repoMatch = DbContext.GetRepository<ProxyMatch>();
            repoMatch.DbContextOptions.EnableCascadeSave= true;
            proxyRoute.ProxyMatchId = repoMatch.InsertAsync(proxyRoute.Match).Result.Id;

            if (!proxyRoute.Match.EnableQueryParameters)
            {
                proxyRoute.Match.QueryParameters = null;
            }
            if (!proxyRoute.Match.EnableHeaders)
            {
                proxyRoute.Match.Headers = null;
            }
            if (!proxyRoute.EnableTransforms)
            {
                proxyRoute.Transforms = null;
            }
            if (!proxyRoute.EnableMetadata)
            {
                proxyRoute.Metadatas = null;
            }
         

            var repo = DbContext.GetRepository<ProxyRoute>();
            repo.DbContextOptions.EnableCascadeSave = true; //需要手工开启
            var ret = await repo.InsertAsync(proxyRoute);

            if (ret is not null)
            {
                _logger.LogInformation("Create proxyRoute Success.");
                ReloadConfig();
                return true;
            }
            return false;
 
        }

        public async Task<bool> Delete(int id)
        {           
            var route = Find(id).Result;

            var repoMatch =  DbContext.GetRepository<ProxyMatch>();
            await repoMatch.DeleteCascadeByDatabaseAsync(c => c.Id == route.ProxyMatchId);

            var repo = DbContext.GetRepository<ProxyRoute>();
            var res = await repo.DeleteCascadeByDatabaseAsync(c => c.Id == id);
            if (res is not null && res.Count() > 0)
            {
                _logger.LogInformation($"Delete proxyRoute: {id} Success.");
                ReloadConfig();
                return true;
            }
            return false;

        }

        public async Task<ProxyRoute> Find(int id)
        {

            return await DbContext.Select<ProxyRoute>()
                .Include(r => r.Match)
                .Include(r => r.Cluster)
                .IncludeMany(r => r.Metadatas)
                .IncludeMany(r => r.Transforms)
                .IncludeMany(r => r.Match.QueryParameters)
                .IncludeMany(r => r.Match.Headers)
                .Where(r => r.Id == id)
                .FirstAsync();

        }

        public ISelect<ProxyRoute> GetAll()
        {
            return DbContext.Select<ProxyRoute>()
               .Include(r => r.Match)
                .Include(r => r.Cluster)
                .IncludeMany(r => r.Metadatas)
                .IncludeMany(r => r.Transforms)
                .IncludeMany(r => r.Match.QueryParameters)
            .IncludeMany(r => r.Match.Headers);
            //.IncludeMany(r => r.Match.Methods);

        }

        public async Task<bool> Update(ProxyRoute proxyRoute)
        {
            var repoMatch = DbContext.GetRepository<ProxyMatch>();
            repoMatch.DbContextOptions.EnableCascadeSave = true;

            if (!proxyRoute.Match.EnableQueryParameters)
            {
                proxyRoute.Match.QueryParameters = null;
            }
            if (!proxyRoute.Match.EnableHeaders)
            {
                proxyRoute.Match.Headers = null;
            }
            if (!proxyRoute.EnableTransforms)
            {
                proxyRoute.Transforms = null;
            }
            if (!proxyRoute.EnableMetadata)
            {
                proxyRoute.Metadatas = null;
            }

            proxyRoute.Match.Methods = proxyRoute.Match.Methods.Trim(',');

            DbContext.Delete<RouteHeader>().Where(p => p.ProxyMatchId == proxyRoute.Match.Id).ExecuteAffrows();
            DbContext.Delete<RouteQueryParameter>().Where(p => p.ProxyMatchId == proxyRoute.Match.Id).ExecuteAffrows();
            if (proxyRoute.Match.Headers?.Count > 0)
            {
                foreach (var h in proxyRoute.Match.Headers)
                {
                    h.Id = 0;
                }

            }
            if (proxyRoute.Match.QueryParameters != null)
            {
                foreach (var h in proxyRoute.Match.QueryParameters)
                {
                    h.Id = 0;
                }
            }
  
            await repoMatch.UpdateAsync(proxyRoute.Match);

            //删除 Metadatas
            DbContext.Delete<Metadata>().Where(p => p.ProxyRouteId == proxyRoute.Id).ExecuteAffrows();
            if (proxyRoute.Metadatas?.Count() > 0)
            {
                foreach (var des in proxyRoute.Metadatas)
                {
                    des.Id = 0;
                }
            }

            var repo = DbContext.GetRepository<ProxyRoute>();
            repo.DbContextOptions.EnableCascadeSave = true; //需要手工开启

   
            //删除 Transforms
            DbContext.Delete<Transform>().Where(p => p.ProxyRouteId == proxyRoute.Id).ExecuteAffrows();
            if (proxyRoute.Transforms?.Count() > 0)
            {
                foreach (var des in proxyRoute.Transforms)
                {
                    des.Id = 0;
                }
            }

            var res = await repo.UpdateAsync(proxyRoute);
            ReloadConfig();
            return res > 0;

        }
        private void ReloadConfig()
        {
            Task.Factory.StartNew(() => _reverseProxyStore.Reload());
            if (_gubonSettings.GubonHttpCounter.StoreInRedis)
            {
                _redisClient.PublishAsync($"{_gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayReloadConfig}", _gubonSettings.GubonHttpCounter.ServiceName);
            }
        }
    }
}
