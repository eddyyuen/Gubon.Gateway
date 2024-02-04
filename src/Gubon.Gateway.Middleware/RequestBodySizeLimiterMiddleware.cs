using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Utils.HttpContextHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Buffers;
using Gubon.Gateway.Utils.Config;
using Microsoft.AspNetCore.Builder;

namespace Gubon.Gateway.Middleware
{
    public  class RequestBodySizeLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        // Supplied via DI
        private readonly ILogger<LogsMiddleware> _logger;
        private readonly GubonSettings _gubonSettings;
        public RequestBodySizeLimiterMiddleware(RequestDelegate next, ILogger<LogsMiddleware> logger, GubonSettings gubonSettings)
        {
            _logger = logger;
            _next = next;
            _gubonSettings = gubonSettings;
        }

        /// <summary>
        /// Entrypoint for being called as part of the request pipeline
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        { // 检查主体大小
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength > _gubonSettings.MaxRequestBodySize) // 设置你的限制大小
            {
                _logger.LogWarning("请求的主体大小超过限制");
                context.Response.StatusCode = 413; // 请求实体过大
                await context.Response.WriteAsync("请求的主体大小超过限制");
                return;
            }
            await _next(context);
        }

   
    }
}
