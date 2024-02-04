using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gubon.Gateway.Store.FreeSql.Models.Dto
{

    public class AccountLogin
    {
        public string Account { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

    }
}
