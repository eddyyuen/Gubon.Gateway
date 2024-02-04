using Gubon.Gateway.Utils.Config;
using Gubon.Gateway.Middleware.ContextStates;
using Gubon.Gateway.Middleware.CustomMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text.Json;
using Gubon.Gateway.Utils.HttpContextHelper;

namespace Gubon.Gateway.Middleware
{
    public static class MiddlewareExtentions
    {
        /// <summary>
        /// 记录网关访问的次数
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGubonHttpCounter(this IReverseProxyApplicationBuilder builder)
        {

            builder.UseMiddleware<RecordDesinationsStates>();
            return builder;
        }


        /// <summary>
        /// 记录网关请求日志和返回日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGubonHttpLog(this IReverseProxyApplicationBuilder builder, GubonSettings gubonSettings)
        {
            if (gubonSettings.Middlewares.UseGubonHttpLog)
            {
                builder.UseMiddleware<LogsMiddleware>();
            }
            return builder;
        }
        /// <summary>
        /// 记录网关异常日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGubonErrorHandler(this IReverseProxyApplicationBuilder builder, GubonSettings gubonSettings)
        {
            if (gubonSettings.Middlewares.UseGubonErrorHandler)
            {
                builder.UseMiddleware<ErrorHandleMiddleware>();
            }
            return builder;
        }
        /// <summary>
        /// 设置内部错误的返回数据格式
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGubonExceptionHandler(this IApplicationBuilder builder, GubonSettings gubonSettings)
        {
            if (!gubonSettings.Middlewares.UseGubonExceptionHandler)
            {
                return builder;
            }

            builder.UseExceptionHandler(configure =>
            {
                configure.Run(async context =>
                {
                    var exHeader = context.Features.Get<IExceptionHandlerPathFeature>();
                    var ex = exHeader?.Error;
                    if (ex is not null)
                    {

                        Log.Logger.ForContext<ErrorHandleMiddleware>().Warning("ErrorType:{ErrorType},Scheme:{Scheme},Host:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize},StatusCode:{StatusCode},Exception:{exception}"

                           , "ExceptionHandler", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength, context.Response.StatusCode, ex);
                        context.Response.StatusCode = 500;


                        context.Response.StatusCode = gubonSettings.GubonException.StatusCode;
                        context.Response.Headers.ContentType = gubonSettings.GubonException.ContentType;
                        string jsonFormatted = JsonSerializer.Serialize(ex.Message, new JsonSerializerOptions
                        {
                            WriteIndented = true // 设置缩进和换行
                        });
                        await context.Response.WriteAsync(gubonSettings.GubonException.ResponeBody.Replace("{exception}", jsonFormatted));


                    }
                });
            });

            return builder;
        }

        /// <summary>
        /// 设置400-599错误的返回数据格式
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGubontatusCodePagesHandler(this IApplicationBuilder builder, GubonSettings gubonSettings)
        {
            if (!gubonSettings.Middlewares.UseGubontatusCodePagesHandler)
            {
                return builder;
            }

            builder.UseStatusCodePages(configure =>
            {
                configure.Run(async context =>
                {
                    Log.Logger.ForContext<ErrorHandleMiddleware>().Warning("ErrorType:{ErrorType},Scheme:{Scheme},:{Host},Path:{Path},QueryString:{QueryString},Method:{Method},RemoteIp:{RemoteIp},BodySize:{BodySize},StatusCode:{StatusCode},"
                      , "StatusCodePages", context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, context.Request.Method, RemoteIpHelper.GetRemoteIp(context), context.Request.ContentLength, context.Response.StatusCode);
                    //context.Response.StatusCode = context.Response.StatusCode;
                    //await context.Response.WriteAsJsonAsync(new { stauscode = context.Response.StatusCode, code = 1, errPath = context.Request.Path.ToString(), queryString = context.Request.QueryString.ToString() });

                });
            });

            return builder;
        }

        /// <summary>
        /// 限制RequestBodySize
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="gubonSettings"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestBodySizeLimiter(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestBodySizeLimiterMiddleware>();
            return builder;
        }
    }
}