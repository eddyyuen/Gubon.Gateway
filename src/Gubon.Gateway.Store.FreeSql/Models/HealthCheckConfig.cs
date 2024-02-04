using FreeSql;
using FreeSql.DataAnnotations;
using System.Security.Principal;

namespace Gubon.Gateway.Store.FreeSql.Models
{
    [Table(Name = "HealthCheckConfig")]
    public class HealthCheckConfig
    {

        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public string? AvailableDestinationsPolicy { get; set; }

        public bool EnableActive { get; set; }
        public bool EnablePassive { get; set; }

        public int ActiveId { get; set; }
        public int PassiveId { get; set; }

        [Navigate(nameof(ActiveId))]
        public HealthCheckActive? Active { get; set; }

        [Navigate(nameof(PassiveId))]
        public HealthCheckPassive? Passive { get; set; }
    }
}
