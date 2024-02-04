using FreeSql;
using FreeSql.DataAnnotations;
using System.Security.Principal;

namespace Gubon.Gateway.Store.FreeSql.Models
{
    [Table(Name = "HealthCheckActive")]
    public class HealthCheckActive
    {
        
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// Whether active health checks are enabled.
        /// </summary>
        //public bool Enabled { get; set; } = false; 

        /// <summary>
        /// Health probe interval.
        /// </summary>
        public string Interval { get; set; } = string.Empty;

        /// <summary>
        /// Health probe timeout, after which a destination is considered unhealthy.
        /// </summary>
        public string Timeout { get; set; } = string.Empty;

        /// <summary>
        /// Active health check policy.
        /// </summary>
        public string Policy { get; set; } = string.Empty;

        /// <summary>
        /// HTTP health check endpoint path.
        /// </summary>
        public string Path { get; set; } = string.Empty;
        //public int? HealthCheckOptionsId { get; set; }

        //[Navigate(nameof(HealthCheckOptionsId))]
        //public virtual HealthCheckOptions? HealthCheckOptions { get; set; }
    }
}
