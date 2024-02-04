using Gubon.Gateway.Store.FreeSql.Models;
using FreeSql;
using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "RouteQueryParameter")]
    public class RouteQueryParameter
    {
           [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the query parameter to look for.
        /// This field is case insensitive and required.
        /// </summary>
        public string? Name { get; init; } = default!;

        /// <summary>
        /// A collection of acceptable query parameter values used during routing.
        /// </summary>
        public string? Values { get; init; } = string.Empty;

        /// <summary>
        /// Specifies how query parameter values should be compared (e.g. exact matches Vs. contains).
        /// Defaults to <see cref="QueryParameterMatchMode.Exact"/>.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public QueryParameterMatchMode Mode { get; init; }

        /// <summary>
        /// Specifies whether query parameter value comparisons should ignore case.
        /// When <c>true</c>, <see cref="StringComparison.Ordinal" /> is used.
        /// When <c>false</c>, <see cref="StringComparison.OrdinalIgnoreCase" /> is used.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool IsCaseSensitive { get; init; }
        public int ProxyMatchId { get; set; }
        //[Navigate( nameof(ProxyMatchId))]
        //public virtual ProxyMatch? ProxyMatch { get; set; }
    }
}
