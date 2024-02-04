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
    public class HeaderValidator : AbstractValidator<RouteHeader>
    {

        public HeaderValidator() {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Header名称不能为空");
            RuleFor(x => x.Values).NotEmpty().WithMessage("Header值不能为空");
            RuleFor(x => x.Mode).NotNull().WithMessage("Header模式不能为空");
        }

 
    }
}
