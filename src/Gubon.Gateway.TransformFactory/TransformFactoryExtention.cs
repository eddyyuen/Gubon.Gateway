using Microsoft.Extensions.DependencyInjection;

namespace Gubon.Gateway.TransformFactory
{
    public static class TransformFactoryExtention
    {
        public static IReverseProxyBuilder AddTransformFactories( this IReverseProxyBuilder builder)
        {
            builder.AddTransformFactory<Json.JsonTransformFactory>();
            return builder;
        }
    }
}