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
    public class ClusterValidator : AbstractValidator<Cluster>
    {

        public ClusterValidator() {
            this.ClassLevelCascadeMode = CascadeMode.Stop;
        
            RuleFor(c=>c.ClusterName).NotEmpty().WithMessage("集群名称不能为空");
            RuleFor(c => c.LoadBalancingPolicy).Must(BuiltIn.LoadBalancingPolicyType.Contains).WithMessage("负载均衡策略不正确");
            RuleFor(c => c.Destinations).NotEmpty().WithMessage("目的地不能为空");
            RuleFor(c => c.HttpRequest).NotEmpty().WithMessage("HttpRequest 不能为空").When(c => c.EnableHttpRequest == true);
            RuleFor(c => c.HttpClient).NotEmpty().WithMessage("HttpClient 不能为空").When(c => c.EnableHttpClient == true);
            RuleFor(c => c.SessionAffinity).NotEmpty().WithMessage("SessionAffinity 不能为空").When(c => c.EnableSessionAffinity == true);
            RuleFor(c => c.Metadatas).NotEmpty().WithMessage("元数据 不能为空").When(c => c.EnableMetadata == true);

            RuleForEach(c => c.Destinations).SetValidator(new DestinationsValidator());
            RuleFor(c => c.HttpRequest).SetValidator(new HttpRequestValidator()).When(c => c.EnableHttpRequest && c.HttpRequest != null);
            RuleFor(c => c.HttpClient).NotNull().SetValidator(new HttpClientValidator()).When(c=>c.EnableHttpClient && c.HttpClient != null);
            RuleFor(c => c.SessionAffinity).SetValidator(new SessionAffinityValidator()).When(c => c.EnableSessionAffinity && c.SessionAffinity is not null);
            RuleForEach(c => c.Metadatas).SetValidator(new MetadataValidator()).When(c =>c.EnableMetadata && c.Metadatas is not null);

        }

 
    }
}
