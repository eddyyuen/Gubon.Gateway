using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gubon.Gateway.TransformFactory.Json
{
    public class JsonTransformFactory : ITransformFactory
    {
        public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue("TransformJson", out var value))
            {
                if (value == "True")
                {
                    context.AddRequestTransform(async requestContext =>
                    {

                        //var code1 = requestContext.HttpContext.Request.RouteValues["code"]?.ToString();

                        using var reader = new StreamReader(requestContext.HttpContext.Request.Body);
                        var body = await reader.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(body))
                        {
                            //todo 获取body中的 code
                            var matches = new Regex("\"code\":\"(?<code>[a-zA-Z0-9-_]*)\",", RegexOptions.Singleline).Matches(body);
                            if (matches.Count > 0)
                            {
                                var code = matches[0].Groups["code"].Value;
                                requestContext.Path = $"/api/an1/{code}/_json";
                            }
                            var RemoteIp = requestContext.HttpContext.Connection.RemoteIpAddress?.ToString();
                            var X_Forwarded_For = requestContext.HttpContext.Request.Headers["X-Forwarded-For"];

                            if (StringValues.IsNullOrEmpty(X_Forwarded_For))
                            {
                                var X_Real_IP = requestContext.HttpContext.Request.Headers["X-Real-IP"];
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
                            //添加RemoteIp
                            body = string.Format(@"[{0},""ip"":""{1}""}}]", body[..^1], RemoteIp);
                            //body 头尾添加[]
                            //body = string.Concat("[", body, "]");
                            var bytes = Encoding.UTF8.GetBytes(body);
                            requestContext.HttpContext.Request.Body = new MemoryStream(bytes);
                            if (requestContext.ProxyRequest.Content is not null)
                            {
                                requestContext.ProxyRequest.Content.Headers.ContentLength = bytes.Length;
                            }

                        }



                    });
                }

                return true;
            }

            return false;
        }

        public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue("TransformJson", out var value))
            {
                if (string.IsNullOrEmpty(value))
                {
                    context.Errors.Add(new ArgumentException("A non-empty TransformJson value is required"));
                }

                return true; // Matched
            }
            return false;
        }
    }
}
