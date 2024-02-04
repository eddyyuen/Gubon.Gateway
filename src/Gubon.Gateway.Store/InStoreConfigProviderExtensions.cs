using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace Gubon.Gateway.Store
{
    public static class InStoreConfigProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromStore(this IReverseProxyBuilder builder)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
            {
                return new InStoreConfigProvider(sp.GetService<ILogger<InStoreConfigProvider>>(), sp.GetRequiredService<IReverseProxyStore>());
            });
            return builder;
        }
    }
}
