using FreeSql;
using System;
using FreeSql.DataAnnotations;
using Yarp.ReverseProxy.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "RouteHeader")]
    public class RouteHeader
    {
           [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// Name of the header to look for.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A collection of acceptable header values used during routing. Only one value must match.
        /// The list must not be empty unless using <see cref="HeaderMatchMode.Exists"/>.
        /// </summary>
        public string Values { get; set; } = string.Empty;

        /// <summary>
        /// Specifies how header values should be compared (e.g. exact matches Vs. by prefix).
        /// Defaults to <see cref="HeaderMatchMode.ExactHeader"/>.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public HeaderMatchMode Mode { get; set; }

        /// <summary>
        /// Specifies whether header value comparisons should ignore case.
        /// When <c>true</c>, <see cref="StringComparison.Ordinal" /> is used.
        /// When <c>false</c>, <see cref="StringComparison.OrdinalIgnoreCase" /> is used.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        public int ProxyMatchId { get; set; }

        //[Navigate(nameof(ProxyMatchId))]
        //public virtual ProxyMatch? ProxyMatch { get; set; }
    }
}
