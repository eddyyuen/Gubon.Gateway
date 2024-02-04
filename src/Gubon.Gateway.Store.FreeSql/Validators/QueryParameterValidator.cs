using FluentValidation;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Models.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.LoadBalancing;
 

namespace Gubon.Gateway.Store.FreeSql.Validate
{
    public class QueryParameterValidator : AbstractValidator<RouteQueryParameter>
    {

        public QueryParameterValidator() {
            RuleFor(x => x.Name).NotEmpty().WithMessage("查询参数名称不能为空");
            RuleFor(x => x.Values).NotEmpty().WithMessage("查询参数值不能为空");
            RuleFor(x => x.Mode).NotNull().WithMessage("查询参数模式不能为空");
        }

 
    }
}
