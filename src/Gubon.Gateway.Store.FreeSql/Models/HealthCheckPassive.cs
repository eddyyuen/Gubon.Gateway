using FreeSql;
using FreeSql.DataAnnotations;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "HealthCheckPassive")]
    public class HealthCheckPassive
    {
           [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// Whether passive health checks are enabled.
        /// </summary>
        //public bool Enabled { get; set; } = false;

        /// <summary>
        /// Passive health check policy.
        /// </summary>
        public string Policy { get; set; } =string.Empty;

        /// <summary>
        /// Destination reactivation period after which an unhealthy destination is considered healthy again.
        /// </summary>
        public string ReactivationPeriod { get; set; } = string.Empty;
        //public int HealthCheckOptionsId { get; set; }


        //[Navigate(nameof(HealthCheckOptionsId))]
        //public virtual HealthCheckOptions? HealthCheckOptions { get; set; }
    }
}
