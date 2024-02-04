using Gubon.Gateway.Store.FreeSql.Models.Dto;

namespace Gubon.Gateway
{
    public static class GubonInfo
    {
        private static GateWayInfo gateWayInfo = new GateWayInfo();
        public static GateWayInfo Instance
        {
            get { return gateWayInfo; }
        }

    }
}
