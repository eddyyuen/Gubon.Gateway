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
    public class DestinationsValidator : AbstractValidator<Destination>
    {

        public DestinationsValidator() {
            RuleFor(d => d.DestName).NotEmpty().WithMessage("目的地名称不能为空");
            RuleFor(d => d.Address).NotEmpty().WithMessage("目的地域名/IP不能为空")
                .Must(IsURL).WithMessage("目的地域名/IP格式不正确");
            RuleFor(d => d.Health).NotEmpty().When(d=>!string.IsNullOrEmpty(d.Health)).WithMessage("check url1");
        }
        private bool IsURL(string url)
        {
            var pattern = @"^(https?)://([A-Za-z0-9])+([A-Za-z0-9\-])*([A-Za-z0-9\-\.])+([A-Za-z0-9])+(:\d+)?/?$";
            return Regex.IsMatch(url, pattern);
        }
    
 
    }
}
