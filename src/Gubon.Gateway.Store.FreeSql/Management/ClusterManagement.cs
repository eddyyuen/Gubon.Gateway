using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.Extensions.Logging;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeRedis;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Gubon.Gateway.Utils.Config;

namespace Gubon.Gateway.Store.FreeSql.Management
{
    public class ClusterManagement : IClusterManagement
    {
        private readonly ILogger<ClusterManagement> _logger;
        private IFreeSql DbContext;
        private readonly IReverseProxyStore _reverseProxyStore;
        private readonly IRedisClient? _redisClient;
        private readonly GubonSettings _gubonSettings;
        public ClusterManagement(IFreeSql dbContext, IReverseProxyStore reverseProxyStore, ILogger<ClusterManagement> logger,
            IRedisClient? redisClient,GubonSettings gubonSetting)
        {
            DbContext = dbContext;
            _reverseProxyStore = reverseProxyStore;
            _logger = logger;
            _redisClient = redisClient;
            _gubonSettings = gubonSetting;
        }

        public async Task<bool> Create(Cluster cluster)
        {   
            using var uow = DbContext.CreateUnitOfWork();

            //检查是否重复
                         
            var repo = DbContext.GetRepository<Cluster>();
            repo.UnitOfWork= uow;
            repo.DbContextOptions.EnableCascadeSave = true; //需要手工开启

            

            //删除 HealthCheck
            if (cluster.HealthCheckConfig != null)
            {
              
                if (cluster.HealthCheckConfig.EnablePassive && cluster.HealthCheckConfig.Passive is not null)
                {
                     
                    var PassiveRepo = DbContext.GetRepository<HealthCheckPassive>();
                    PassiveRepo.UnitOfWork =uow;
                    cluster.HealthCheckConfig.PassiveId = PassiveRepo.InsertAsync(cluster.HealthCheckConfig.Passive).Result.Id;
                }
                if (cluster.HealthCheckConfig.EnableActive && cluster.HealthCheckConfig.Active is not null)
                {
                    var ActiveRepo = DbContext.GetRepository<HealthCheckActive>();
                    ActiveRepo.UnitOfWork = uow;
                    cluster.HealthCheckConfig.ActiveId = ActiveRepo.InsertAsync(cluster.HealthCheckConfig.Active).Result.Id;
                 
                }
                var healthCheckConfigRepo = DbContext.GetRepository<HealthCheckConfig>();
                healthCheckConfigRepo.UnitOfWork =uow;
                cluster.HealthCheckConfigId = healthCheckConfigRepo.InsertAsync(cluster.HealthCheckConfig).Result.Id;
            }
            if (cluster.EnableHttpRequest && cluster.HttpRequest is not null)
            {
                var Repo = DbContext.GetRepository<ForwarderRequest>();
                Repo.UnitOfWork = uow;
                cluster.HttpRequestId=  Repo.InsertAsync(cluster.HttpRequest).Result.Id;
            }
            if (cluster.EnableHttpClient && cluster.HttpClient is not null)
            {
                var Repo = DbContext.GetRepository<HttpClientConfig>();
                Repo.UnitOfWork = uow;
                cluster.HttpClientId = Repo.InsertAsync(cluster.HttpClient).Result.Id;
            }

            if(cluster.EnableSessionAffinity && cluster.SessionAffinity is not null)
            {
                var Repo = DbContext.GetRepository<SessionAffinityConfig>();
                Repo.UnitOfWork = uow;
                cluster.SessionAffinityId = Repo.InsertAsync(cluster.SessionAffinity).Result.Id;
            }
           
            if(cluster.EnableMetadata)
            {
                cluster.Metadatas = cluster.Metadatas?.Where(x => string.IsNullOrWhiteSpace(x.Key) && string.IsNullOrWhiteSpace(x.Value)).ToList();
                if (cluster.Metadatas?.Count() > 0)
                {
                    foreach (var des in cluster.Metadatas)
                    {
                        des.Id = 0;
                    }
                }               
            }
            else
            {
                cluster.Metadatas = null;
            }
            

            var ret =  await repo.InsertAsync(cluster);
            
            if (ret is not null)
            {
                uow.Commit();
                _logger.LogInformation("Create Cluster Success.");
                ReloadConfig();
                return true;
            }
            uow.Rollback();
            return false;
        }

        public async Task<bool> Delete(int id)
        {
            var repo = DbContext.GetRepository<Cluster>();

            var cluster = Find(id).Result;

            if (cluster == null)
                return true;

            if(cluster.HealthCheckConfig is not null)
            {
                if(cluster.HealthCheckConfig.ActiveId > 0)
                {
                    await DbContext.Delete<HealthCheckActive>().Where(c=>c.Id==cluster.HealthCheckConfig.ActiveId).ExecuteAffrowsAsync();
                }
                if (cluster.HealthCheckConfig.PassiveId > 0)
                {
                    await DbContext.Delete<HealthCheckPassive>().Where(c => c.Id == cluster.HealthCheckConfig.PassiveId).ExecuteAffrowsAsync();
                }
                await DbContext.Delete<HealthCheckConfig>().Where(c => c.Id == cluster.HealthCheckConfigId).ExecuteAffrowsAsync();
            }

            if(cluster.HttpRequest is not null) {
               await DbContext.Delete<ForwarderRequest>().Where(c => c.Id == cluster.HttpRequestId).ExecuteAffrowsAsync();
            }

            if (cluster.HttpClient is not null)
            {
                await DbContext.Delete<HttpClientConfig>().Where(c => c.Id == cluster.HttpClientId).ExecuteAffrowsAsync();
            }

            if (cluster.SessionAffinity is not null)
            {
                await DbContext.Delete<SessionAffinityConfig>().Where(c => c.Id == cluster.SessionAffinityId).ExecuteAffrowsAsync();
            }

            var res = await repo.DeleteCascadeByDatabaseAsync(c=>c.Id== id);

             
            if (res is not null && res.Count()>0)
            {
                _logger.LogInformation($"Delete Cluster: {id} Success.");
                ReloadConfig();
                return true;
            }
            return false;
        }

        public async Task<Cluster> Find(int id)
        {
            return await DbContext.Select<Cluster>()
            .IncludeMany(c => c.Destinations)
            .Include(c => c.HttpClient)
            .IncludeMany(c => c.Metadatas)
            .Include(c => c.HttpRequest)
            .Include(c => c.SessionAffinity)
            .Include(c=>c.HealthCheckConfig.Active)
            .Include(c=>c.HealthCheckConfig.Passive)
            .Where(c=>c.Id== id)
            .FirstAsync();

        }

        public ISelect<Cluster> GetAll()
        {
            //return DbContext.GetRepository<Cluster>();
            return DbContext.Select<Cluster>();
            
        //    .IncludeMany(c => c.HealthCheck, then => then.IncludeMany(d => d.Active).IncludeMany(d => d.Passive))
        //    //   .Include(c => c.HealthCheck)
        //    //.IncludeMany(c => c.HealthCheck.Active)
        //    //.IncludeMany(c => c.HealthCheck.Passive)
        //    .IncludeMany(c => c.HttpClient, then => then.IncludeMany(d => d.WebProxy))
        //    .IncludeMany(c => c.SessionAffinity, then => then.IncludeMany(d => d.Cookie))
        //    .IncludeMany(c => c.Metadatas)
        //    .IncludeMany(c => c.HttpRequest);
        }

        public async Task<bool> Update(Cluster cluster)
        {
            
            var repo = DbContext.GetRepository<Cluster>();
            repo.DbContextOptions.EnableCascadeSave = true; //需要手工开启

            //更新 HealthCheck
            if (cluster.HealthCheckConfig!=null)
            {               
                if (cluster.HealthCheckConfig.EnablePassive && cluster.HealthCheckConfig.Passive !=null)
                {
                    cluster.HealthCheckConfig.PassiveId = DbContext.GetRepository<HealthCheckPassive>().InsertOrUpdateAsync(cluster.HealthCheckConfig.Passive).Result.Id;
                }
                if (cluster.HealthCheckConfig.EnableActive && cluster.HealthCheckConfig.Active!= null)
                {
                    cluster.HealthCheckConfig.ActiveId =  DbContext.GetRepository<HealthCheckActive>().InsertOrUpdateAsync(cluster.HealthCheckConfig.Active).Result.Id;
                }
                await DbContext.GetRepository<HealthCheckConfig>().UpdateAsync(cluster.HealthCheckConfig);
            }


            //更新 HttpRequest
            if (cluster.EnableHttpRequest && cluster.HttpRequest != null)
            {
                cluster.HttpRequestId = DbContext.GetRepository<ForwarderRequest>().InsertOrUpdateAsync(cluster.HttpRequest).Result.Id;
            }
            //更新 HttpClient
            if (cluster.EnableHttpClient && cluster.HttpClient != null )
            {
                cluster.HttpClientId = DbContext.GetRepository<HttpClientConfig>().InsertOrUpdateAsync(cluster.HttpClient).Result.Id;

            }
      


            //更新 SessionAffinity
            if (cluster.EnableSessionAffinity && cluster.SessionAffinity != null)
            {
                if (!cluster.SessionAffinity.Cookie)
                {
                    cluster.SessionAffinity.CookieDomain = null;
                    cluster.SessionAffinity.CookiePath = null;
                    cluster.SessionAffinity.CookieSecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                    cluster.SessionAffinity.CookieExpiration = null;
                    cluster.SessionAffinity.CookieHttpOnly = false;
                    cluster.SessionAffinity.CookieIsEssential = false;
                    cluster.SessionAffinity.CookieMaxAge = null;
                    cluster.SessionAffinity.CookieSameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                }

                cluster.SessionAffinityId = DbContext.GetRepository<SessionAffinityConfig>().InsertOrUpdateAsync(cluster.SessionAffinity).Result.Id;

            }

            //删除 Destinations
            DbContext.Delete<Destination>().Where(p => p.ClusterId == cluster.Id).ExecuteAffrows();
            if (cluster.Destinations?.Count() > 0)
            {
                foreach (var des in cluster.Destinations)
                {
                    des.Id = 0;
                }
            }

            //删除 Metadatas
            DbContext.Delete<Metadata>().Where(p => p.ClusterId == cluster.Id).ExecuteAffrows();
            if (!cluster.EnableMetadata)
            {
                cluster.Metadatas = null;
            }
            if (cluster.Metadatas?.Count() > 0)
            {
                foreach (var des in cluster.Metadatas)
                {
                    des.Id = 0;
                }
            }

            var res = await repo.UpdateAsync(cluster);
            ReloadConfig();
            return res>0;

        }
        private void ReloadConfig()
        {
            Task.Factory.StartNew(() => _reverseProxyStore.Reload());
            if (_gubonSettings.GubonHttpCounter.StoreInRedis)
            {
                _redisClient?.PublishAsync($"{_gubonSettings.GubonHttpCounter.ClusterName}:{RedisKey.GubonGatewayReloadConfig}", _gubonSettings.GubonHttpCounter.ServiceName);
            }
        }

   
    }
}
