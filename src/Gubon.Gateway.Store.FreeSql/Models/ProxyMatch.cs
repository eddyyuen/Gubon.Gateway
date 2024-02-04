using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FreeSql;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "ProxyMatch")]
    public class ProxyMatch
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// Only match requests that use these optional HTTP methods. E.g. GET, POST.
        /// </summary>
       // public string Methods { get; set; } = string.Empty;

        /// <summary>
        /// Only match requests with the given Host header.
        /// </summary>
        public string Hosts { get; set; } = string.Empty;

        /// <summary>
        /// Only match requests with the given Path pattern.
        /// </summary>
        public string Path { get; set; } = "/{**url}";

        public bool EnableQueryParameters { get; set; }
        public bool EnableHeaders { get; set; }

        public string Methods { get; set; } = string.Empty;
        /// <summary>
        /// Only match requests that contain all of these query parameters.
        // [Navigate(nameof(RouteQueryMethod.ProxyMatchId))]
        //  public virtual List<RouteQueryMethod>? Methods { get; set; }
        //public bool ShouldSerializeMethods() => Methods != null && Methods.Count > 0;
        /// </summary>

        [Navigate(nameof(RouteQueryParameter.ProxyMatchId))]
        public virtual List<RouteQueryParameter>? QueryParameters { get; set; }
        //public bool ShouldSerializeQueryParameters() => QueryParameters != null && QueryParameters.Count > 0;

        [Navigate(nameof(RouteHeader.ProxyMatchId))]
        public virtual List<RouteHeader>? Headers { get; set; }
        //public bool ShouldSerializeHeaders() => Headers != null && Headers.Count > 0;


    }


}