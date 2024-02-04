using FreeSql;
using FreeSql.DataAnnotations;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "Metadata")]
    public class Metadata : KeyValueEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public int ClusterId { get; set; }
        public int DestinationId { get; set; }
        public int ProxyRouteId { get; set; }
    }
}
