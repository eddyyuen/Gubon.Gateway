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
    public class SessionAffinityValidator : AbstractValidator<SessionAffinityConfig>
    {
        public SessionAffinityValidator()
        {

            RuleFor(x => x.AffinityKeyName).NotEmpty().WithMessage("亲和性主键名称不能为空");
            RuleFor(x => x.Policy).NotEmpty().Must(BuiltIn.PolicyList.Contains).WithMessage("策略不存在");
            RuleFor(x => x.FailurePolicy).NotEmpty().Must(BuiltIn.FailurePolicyList.Contains).WithMessage("错误策略不存在");

            RuleFor(x => x.CookieExpiration).Must(IsTimespan).When(x=>x.Cookie).WithMessage("过期时间格式错误");
            RuleFor(x => x.CookieMaxAge).Must(IsTimespan).When(x => x.Cookie).WithMessage("最大生存时间格式错误");


        }
        private bool IsTimespan(string value)
        {
            try
            {
                var t = TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture);
                return t.Ticks > 0;
            }
            catch { return false; }
        }
    }
}
