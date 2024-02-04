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
    public class TransformsValidator : AbstractValidator<Transform>
    {

        public TransformsValidator()
        {
            RuleFor(x => x.Key).NotEmpty().WithMessage("[转换配置]名称不能为空");
            RuleFor(x => x.Value).NotEmpty().WithMessage("[转换配置]值不能为空");
            RuleFor(x => x.Type).NotEmpty().WithMessage("[转换配置]类型不能为空");
            RuleFor(x => x.Key).Must(IsKey).WithMessage("[转换配置]名称不在允许范围").When(x => x.Type != TransformType.Custom);
        }
        private bool IsKey(string key)
        {
             var ret = Enum.TryParse<TransformType>(key,true,out var keyType);
            if(ret == false)
            {
                ret =  key.Equals("Append", StringComparison.Ordinal) || key.Equals("Set");
            }

           return ret;
        }


    }
}
