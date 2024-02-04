using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FreeSql;
using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql.Models
{


    [Table(Name = "CertificateConfig")]
    public class CertificateConfig

    {
          [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Path { get; set; }

        public string KeyPath { get; set; }

        public string Password { get; set; }

        public string Subject { get; set; }

        public string Store { get; set; }

        public string Location { get; set; }

        public bool? AllowInvalid { get; set; }

        internal bool IsFileCert => !string.IsNullOrEmpty(Path);

        internal bool IsStoreCert => !string.IsNullOrEmpty(Subject);

        public int ProxyHttpClientOptionsId { get; set; }
        //[Navigate(nameof(ProxyHttpClientOptionsId))]
        //public virtual HttpClientConfig ProxyHttpClientOptions { get; set; }
    }
}
