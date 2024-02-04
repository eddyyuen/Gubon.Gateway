using Gubon.Gateway.Store.FreeSql.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Gubon.Gateway.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,  IJwtUtils jwtUtils,IMemoryCache memoryCache, IFreeSql freeSql)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrWhiteSpace(token)) {
                var jwtTokenCode = token.GetHashCode();
                // 从缓存里获取数据
                memoryCache.TryGetValue(jwtTokenCode, out User? user);
                if (user == null)
                {
                    var userAccount = jwtUtils.ValidateJwtToken(token);
                    if (userAccount != null)
                    {
                       var userData = await freeSql.GetRepository<User>().Where(x => x.Account == userAccount && x.Status).FirstAsync();
                        // attach user to context on successful jwt validation
                        context.Items["User"] = userData;
                        memoryCache.Set(jwtTokenCode, userData,  new DateTimeOffset().AddHours(24));
                    }
                }
                else
                {
                    context.Items["User"] = user;
                }
            }

            await _next(context);
        }
    }
}
