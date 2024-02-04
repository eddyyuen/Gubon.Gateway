using Gubon.Gateway.Store.FreeSql.Models;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store
{
    public interface IClusterManagement
    {
        ISelect<Cluster> GetAll();
        Task<Cluster> Find(int id);
        Task<bool> Create(Cluster cluster);
        Task<bool> Update(Cluster cluster);
        Task<bool> Delete(int id);
    }
}
