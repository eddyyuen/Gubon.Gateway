using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FreeSql;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using System.Security.Principal;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "cluster")]
    public class Cluster
    {
        /// <summary>
        /// The Id for this cluster. This needs to be globally unique.
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public string ClusterName { get; set; } = string.Empty;

        /// <summary>
        /// Load balancing policy.
        /// </summary>
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";
  
        public bool EnableSessionAffinity { get; set; }
        public bool EnableHttpClient { get; set; }
        public bool EnableHttpRequest { get; set; }
        public bool EnableMetadata { get; set; }

        public int HealthCheckConfigId { get; set; }

        public int SessionAffinityId { get; set; }

        public int HttpClientId{ get; set; }

        public int HttpRequestId { get; set; }


        [Navigate(nameof(Destination.ClusterId))]
        public virtual List<Destination> Destinations { get; set; } = new List<Destination>();

        [Navigate(nameof(HealthCheckConfigId))]
        public HealthCheckConfig? HealthCheckConfig { get; set; }


        [Navigate(nameof(HttpRequestId))]
        public ForwarderRequest? HttpRequest { get; set; }


        [Navigate(nameof(HttpClientId))]
        public HttpClientConfig? HttpClient { get; set; }

        /// <summary>
        /// Arbitrary key-value pairs that further describe this route.
        /// </summary>
        [Navigate(nameof(Metadata.ClusterId))]
        public virtual List<Metadata>? Metadatas { get; set; }


        /// <summary>
        /// Session affinity options.
        /// </summary>
        [Navigate(nameof(SessionAffinityId))]
        public virtual SessionAffinityConfig? SessionAffinity { get; set; }

    }
}
