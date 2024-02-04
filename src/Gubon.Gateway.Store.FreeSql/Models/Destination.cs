using FreeSql;
using System.Collections.Generic;
using FreeSql.DataAnnotations;

namespace Gubon.Gateway.Store.FreeSql.Models
{
    /// <summary>
    /// Describes a destination of a cluster.
    /// </summary>

    [Table(Name = "Destination")]
    public class Destination
    {
       [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string DestName { get; set; } = string.Empty;
        /// <summary>
        /// Address of this destination. E.g. <c>https://127.0.0.1:123/abcd1234/</c>.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint accepting active health check probes. E.g. <c>http://127.0.0.1:1234/</c>.
        /// </summary>
        public string? Health { get; set; }
        public int ClusterId { get; set; }

        //[Navigate(nameof(ClusterId))]   
        //public virtual Cluster? Cluster { get; set; }

        /// <summary>
        /// Arbitrary key-value pairs that further describe this destination.
        /// </summary>

        //[Navigate(nameof(Metadata.DestinationId))] 
        //public virtual List<Metadata>? Metadatas { get; set; }
        //public bool ShouldSerializeMetadatas() => Metadatas != null && Metadatas.Count > 0;
    }
}
