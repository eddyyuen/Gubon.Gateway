using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace Gubon.Gateway.Store
{
    public interface IReverseProxyStore
    {
        public event ConfigChangeHandler ChangeConfig;
        IProxyConfig GetConfig();

        void Reload();
        void ReloadConfig();
        IChangeToken GetReloadToken();
    }
}
