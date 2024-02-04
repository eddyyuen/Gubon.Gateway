using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Model;

namespace Gubon.Gateway.Store.FreeSql.Models.Dto
{
    public class ClusterStateDto
    {
        public string ClusterId { get; set; } = string.Empty;
        public virtual List<DestinationStateDto> AllDestinations { get; set; } = new List<DestinationStateDto>();
        public virtual List<DestinationStateDto> AvailableDestinations { get; set; } = new List<DestinationStateDto>();
    }
    public class DestinationStateDto
    {
        public bool Available { get; set; }
        public string DestinationId { get; set; }=string.Empty;
        public DestinationHealthState Health { get; set; } = new DestinationHealthState();

        public int ConcurrentRequestCount { get; set; }

        public string Address { get; init; } = string.Empty;


    }
}
