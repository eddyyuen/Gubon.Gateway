using FreeSql;
using System.Collections.Generic;
using FreeSql.DataAnnotations;

namespace Gubon.Gateway.Store.FreeSql.Models
{
    /// <summary>
    /// Describes a route that matches incoming requests based on a the <see cref="Match"/> criteria
    /// and proxies matching requests to the cluster identified by its <see cref="ClusterId"/>.
    /// </summary>

    [Table(Name = "ProxyRoute")]
    public class ProxyRoute
    {
           [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// Globally unique identifier of the route.
        /// </summary>
        public string RouteName { get; set; } = string.Empty;
        public int ClusterId { get; set; }
        [Navigate(nameof(ClusterId))]
        public virtual Cluster? Cluster { get; set; }

        public int ProxyMatchId { get; set; }
        /// <summary>
        /// Parameters used to match requests.
        /// </summary>
        [Navigate( nameof(ProxyMatchId))]
        public virtual ProxyMatch Match { get; set; } 

        /// <summary>
        /// Optionally, an order value for this route. Routes with lower numbers take precedence over higher numbers.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 最大的请求体大小 bytes
        /// </summary>
        public long MaxRequestBodySize { get; set; }

        public string? RateLimiterPolicy { get; set; }

        //[Navigate(nameof(ClusterId))]
        //public virtual Cluster? Cluster { get; set; }

        /// <summary>
        /// The name of the AuthorizationPolicy to apply to this route.
        /// If not set then only the FallbackPolicy will apply.
        /// Set to "Default" to enable authorization with the applications default policy.
        /// Set to "Anonymous" to disable all authorization checks for this route.
        /// </summary>
        public string AuthorizationPolicy { get; set; } = string.Empty;

        /// <summary>
        /// The name of the CorsPolicy to apply to this route.
        /// If not set then the route won't be automatically matched for cors preflight requests.
        /// Set to "Default" to enable cors with the default policy.
        /// Set to "Disable" to refuses cors requests for this route.
        /// </summary>
        public string CorsPolicy { get; set; } = string.Empty;

        public bool EnableMetadata { get; set; }

        /// <summary>
        /// Arbitrary key-value pairs that further describe this route.
        /// </summary>
        [Navigate(nameof(Metadata.ProxyRouteId))]
        public virtual List<Metadata>? Metadatas { get; set; }

        public bool EnableTransforms { get; set; }
        /// <summary>
        /// Parameters used to transform the request and response. See <see cref="Service.ITransformBuilder"/>.
        /// </summary>
        [Navigate(nameof(Transform.ProxyRouteId))]
        public virtual List<Transform>? Transforms { get; set; }

    }
}
