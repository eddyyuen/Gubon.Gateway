namespace Gubon.Gateway.Middlewares
{
    public class ServerHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string ServerHeaderValue = "GubonGateway";

        public ServerHeaderMiddleware(RequestDelegate next,string args)
        {
            _next = next;
            ServerHeaderValue = args;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers["Server"] = ServerHeaderValue;

            await _next(context);
        }
    }

    public static class ServerHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseServerHeader(this IApplicationBuilder builder,string serverName)
        {
            return builder.UseMiddleware<ServerHeaderMiddleware>(serverName);
        }
    }
}
