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
    public class MetadataValidator : AbstractValidator<Metadata>
    {

        public MetadataValidator() {
            RuleFor(x => x.Key).NotEmpty().WithMessage("元数据的 Key 不能为空");
            RuleFor(x => x.Value).NotEmpty().WithMessage("元数据的 Value 不能为空");
        }

 
    }
}
