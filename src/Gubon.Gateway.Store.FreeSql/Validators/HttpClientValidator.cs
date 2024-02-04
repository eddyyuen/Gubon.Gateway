using FluentValidation;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Models.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yarp.ReverseProxy.LoadBalancing;
 

namespace Gubon.Gateway.Store.FreeSql.Validate
{
    public class HttpClientValidator : AbstractValidator<HttpClientConfig>
    {

        public HttpClientValidator() {
            RuleFor(x => x.SslProtocols).NotEmpty().WithMessage("SSL协议不能为空");
            RuleFor(x => x.MaxConnectionsPerServer).GreaterThanOrEqualTo(0).WithMessage("服务器最大连接数不能是负数");
            RuleFor(x => x.WebProxyAddress).NotEmpty().Must(IsURL).When(x => x.WebProxy).WithMessage("代理服务器地址格式错误");
        }

        private bool IsURL(string url)
        {
            var pattern = @"^(https?)://([A-Za-z0-9])+([A-Za-z0-9\-])*([A-Za-z0-9\-\.])+([A-Za-z0-9])+(:\d+)?/?$";
            return Regex.IsMatch(url, pattern);
        }

    }
}
