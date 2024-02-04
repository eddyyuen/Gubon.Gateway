using Gubon.Gateway.Store.FreeSql.Models;
using FreeSql;
using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Http;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "SessionAffinityConfig")]
    public class SessionAffinityConfig
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        /// <summary>
        /// Indicates whether session affinity is enabled.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// The session affinity policy to use.
        /// </summary>
        public string? Policy { get; init; }

        /// <summary>
        /// Strategy handling missing destination for an affinitized request.
        /// </summary>
        public string? FailurePolicy { get; init; }

        /// <summary>
        /// Identifies the name of the field where the affinity value is stored.
        /// For the cookie affinity policy this will be the cookie name.
        /// For the header affinity policy this will be the header name.
        /// The policy will give its own default if no value is set.
        /// This value should be unique across clusters to avoid affinity conflicts.
        /// https://github.com/microsoft/reverse-proxy/issues/976
        /// This field is required.
        /// </summary>
        public string AffinityKeyName { get; init; } = default!;

        public bool Cookie { get; set; }
        public string? CookieDomain { get; set; }
        public string? CookieExpiration { get; set; }
        public bool CookieHttpOnly { get; set; }
        public bool CookieIsEssential { get; set; }
        public string? CookieMaxAge { get; set; }
        public string? CookiePath { get; set; }
        public SameSiteMode? CookieSameSite { get; set; }
        public CookieSecurePolicy? CookieSecurePolicy { get; set; }

    }
}
