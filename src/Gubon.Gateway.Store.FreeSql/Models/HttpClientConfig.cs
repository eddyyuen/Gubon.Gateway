using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FreeSql;
using System;
using FreeSql.DataAnnotations;
using System.Security.Authentication;


namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "HttpClientConfig")]
    public class HttpClientConfig
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        /// <summary>
        /// An empty options instance.
        /// </summary>
        public static readonly HttpClientConfig Empty = new();

        /// <summary>
        /// What TLS protocols to use.
        /// </summary>
        public string? SslProtocols { get; init; }

        /// <summary>
        /// Indicates if destination server https certificate errors should be ignored.
        /// This should only be done when using self-signed certificates.
        /// </summary>
        public bool? DangerousAcceptAnyServerCertificate { get; init; }

        /// <summary>
        /// Limits the number of connections used when communicating with the destination server.
        /// </summary>
        public int? MaxConnectionsPerServer { get; init; }

        /// <summary>
        /// Gets or sets a value that indicates whether additional HTTP/2 connections can
        /// be established to the same server when the maximum number of concurrent streams
        /// is reached on all existing connections.
        /// </summary>
        public bool? EnableMultipleHttp2Connections { get; init; }

        /// <summary>
        /// Enables non-ASCII header encoding for outgoing requests.
        /// </summary>
        public string? RequestHeaderEncoding { get; init; }

        public bool WebProxy { get; init; }
        /// <summary>
        /// The URI of the proxy server.
        /// </summary>
        public string? WebProxyAddress { get; init; }


        /// <summary>
        /// true to bypass the proxy for local addresses; otherwise, false.
        /// If null, default value will be used: false
        /// </summary>
        public bool WebProxyBypassOnLocal { get; init; }

        /// <summary>
        /// Controls whether the <seealso cref="System.Net.CredentialCache.DefaultCredentials"/> are sent with requests.
        /// If null, default value will be used: false
        /// </summary>
        public bool WebProxyUseDefaultCredentials { get; init; }
    }
}
