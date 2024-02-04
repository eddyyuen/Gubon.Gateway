using Gubon.Gateway.Store.FreeSql.Models;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql.Management
{
    public interface IProxyRouteManagement
    {
        ISelect<ProxyRoute> GetAll();
        Task<ProxyRoute> Find(int id);
        Task<bool> Create(ProxyRoute proxyRoute);
        Task<bool> Update(ProxyRoute proxyRoute);
        Task<bool> Delete(int id);
    }
}
