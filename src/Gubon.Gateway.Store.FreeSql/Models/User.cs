using FreeSql;
using System;
using FreeSql.DataAnnotations;
using Yarp.ReverseProxy.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Gubon.Gateway.Store.FreeSql.Models
{

    [Table(Name = "User")]
    public class User
    {
           [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
 
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 

        public bool Status { get; set; }

 
    }
}
