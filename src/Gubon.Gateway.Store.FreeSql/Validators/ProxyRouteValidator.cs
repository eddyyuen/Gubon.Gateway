using FluentValidation;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Store.FreeSql.Models.Constant;
using Gubon.Gateway.Utils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.LoadBalancing;
 

namespace Gubon.Gateway.Store.FreeSql.Validate
{
    public class ProxyRouteValidator : AbstractValidator<ProxyRoute>
    {

        public ProxyRouteValidator() {
            this.ClassLevelCascadeMode = CascadeMode.Stop;
        
            RuleFor(c=>c.RouteName).NotEmpty().WithMessage("路由名称不能为空");
            RuleFor(c => c.ClusterId).GreaterThan(0).WithMessage("集群不能为空");
            RuleFor(c => c.Order).GreaterThan(-1).WithMessage("顺序不能小于0");
            RuleFor(c => c.MaxRequestBodySize).GreaterThan(-1).WithMessage("请求体大小不能小于0");

            RuleFor(c => c.Match).NotEmpty().WithMessage("匹配规则不能为空");
            RuleFor(c => c.Transforms).NotEmpty().WithMessage("转换配置 不能为空").When(c => c.EnableTransforms == true);
            RuleFor(c => c.Metadatas).NotEmpty().WithMessage("元数据 不能为空").When(c => c.EnableMetadata == true);

            RuleFor(c => c.Match.Path).NotEmpty().When(c => c.Match.Hosts.Count() == 0).WithMessage("主机/路径 必须填写其中一个");
            if (AppProvider.GubonSettings.AdminWebSite.Enabled)
            {
                RuleFor(c => c.Match.Path).NotEmpty().Must(x => !x.StartsWith("/{*")).WithMessage("路径必须带有固定的前缀，例如:/apiv2/{**all}");
                RuleFor(c => c.Match.Path).NotEmpty().Must(x => !x.StartsWith("/__admin/")).WithMessage("路径不能以 /__admin 开头，此为系统路由");
            }


            RuleForEach(c => c.Match.Headers).SetValidator(new HeaderValidator()).When(c=>c.Match.EnableHeaders);
            RuleForEach(c => c.Match.QueryParameters).SetValidator(new QueryParameterValidator()).When(c=>c.Match.EnableQueryParameters);

            RuleForEach(c => c.Transforms).SetValidator(new TransformsValidator()).When(c => c.EnableTransforms && c.Transforms is not null);
            RuleForEach(c => c.Metadatas).SetValidator(new MetadataValidator()).When(c =>c.EnableMetadata && c.Metadatas is not null);

        } 
    }
}
