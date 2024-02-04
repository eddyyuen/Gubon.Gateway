using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;

namespace Gubon.Gateway.Utils.HttpContextHelper
{
    public class RemoteIpHelper
    {
        public static string? GetRemoteIp(HttpContext context)
        {
            var RemoteIp = context.Connection.RemoteIpAddress?.ToString();
            var X_Forwarded_For = context.Request.Headers["X-Forwarded-For"];

            if (StringValues.IsNullOrEmpty(X_Forwarded_For))
            {
                var X_Real_IP = context.Request.Headers["X-Real-IP"];
                if (!StringValues.IsNullOrEmpty(X_Real_IP))
                {
                    RemoteIp = X_Real_IP[0];
                }
                else
                {
                    if (RemoteIp is not null)
                    {
                        RemoteIp = new Regex("^.*:").Replace(RemoteIp, "");
                    }
                }
            }
            else
            {
                RemoteIp = X_Forwarded_For[0];
            }
            return RemoteIp;
        }
    }
}
