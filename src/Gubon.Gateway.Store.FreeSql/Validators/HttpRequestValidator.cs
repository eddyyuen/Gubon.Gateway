using FluentValidation;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Models.Constant;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.LoadBalancing;
 

namespace Gubon.Gateway.Store.FreeSql.Validate
{
    public class HttpRequestValidator : AbstractValidator<ForwarderRequest>
    {
        public HttpRequestValidator() {
            RuleFor(x => x.ActivityTimeout).NotEmpty().Must(IsTimespan).WithMessage("超时时间格式错误").When(x=>x.ActivityTimeout !=null);
            RuleFor(x => x.Version).NotEmpty().Must(BuiltIn.RequestVersion.Contains).WithMessage("HTTP 版本错误").When(x=>x.Version !=null);
            RuleFor(x => x.VersionPolicy).NotEmpty().Must(BuiltIn.VersionPolicyType.Contains).WithMessage("HTTP 策略错误").When(x=>x.VersionPolicy != null);


        }
        private bool IsTimespan(string? value)
        {
            try
            {
                if(value == null) return false;
                var t = TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture);
                return t.Ticks > 0;
            }
            catch { return false; }
        }
    
 
    }
}
