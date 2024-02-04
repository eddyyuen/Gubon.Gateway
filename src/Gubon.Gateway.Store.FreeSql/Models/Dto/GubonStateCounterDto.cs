using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql.Models.Dto
{
    public class GubonStateCounterDto
    {
       public DateTime StartDateTime { get; set; }
        public long state_all;
        public long state_2xx;
        public long state_4xx;
        public long state_5xx;
        public List<States>? request_states;
        public List<States>? routes_states;
        public List<States>? destinations_states;
    }
    public record States
    {
        public string Name { get; set; } = string.Empty;
        public long Counter2xx { get; set; }
        public long Counter4xx { get; set; }
        public long Counter400 { get; set; }
        public long Counter401 { get; set; }
        public long Counter403 { get; set; }
        public long Counter405 { get; set; }
        public long Counter5xx { get; set; }
        public long Counter500 { get; set; }
        public long Counter502 { get; set; }
        public long Counter503 { get; set; }
        public long Counter504 { get; set; }


    }

    public class RedisKey
    {
        public static string GubonCounterDestinations
        {
            get {
                return "gubon.counter.destinations";
            }
        }
        public static string GubonCounterRoutes
        {
            get
            {
                return "gubon.counter.routes";
            }
        }
        public static string GubonCounterRequests
        {
            get
            {
                return "gubon.counter.requests";
            }
        }
        public static string GubonCounterTotal
        {
            get
            {
                return "gubon.counter.total";
            }
        }
        public static string GubonGatewayServices
        {
            get
            {
                return "gubon.gateway.services";
            }
        }
        public static string GubonGatewayReloadConfig
        {
            get
            {
                return "gubon.gateway.reloadconfig";
            }
        }
    }

    public record GateWayInfo
    {
        public string ClusterName { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
 
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime{ get; set; } = DateTime.Now;
        /// <summary>
        /// 加载配置时间
        /// </summary>
        public DateTime ReloadTime { get; set; } = DateTime.Now;

    }
}
