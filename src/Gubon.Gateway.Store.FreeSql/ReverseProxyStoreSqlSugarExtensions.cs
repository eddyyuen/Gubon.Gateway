using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql
{
    public static class ReverseProxyStoreFreeSqlExtensions
    {
        public static IReverseProxyBuilder LoadFromFreeSql(this IReverseProxyBuilder builder)
        {
            builder.Services.AddSingleton<IReverseProxyStore, FreeSqlReverseProxyStore>();
            builder.LoadFromStore();
            return builder;
        }
    }
}
