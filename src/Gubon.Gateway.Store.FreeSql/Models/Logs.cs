using FreeSql;
using FreeSql.DataAnnotations;


namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "Logs")]
    public class Logs 
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public DateTime ResponseTime { get; set; }  = DateTime.MinValue;


        public string Method { get; set; } = string.Empty;
        public string? Scheme { get; set; } = "http";
        public string? Host { get; set; } = string.Empty;
        public string? RequestPath { get; set; } = string.Empty;
        public string? Querystring { get; set; } = string.Empty;
        public string? Ip { get; set; } = string.Empty;

        public long? RequestContentLength { get; set; }
        public long? ResponseContentLength { get; set; }
        public int StatusCode { get; set; }

        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public string? Errors { get; set; }
        public string? Exception { get; set; }













    }
}
