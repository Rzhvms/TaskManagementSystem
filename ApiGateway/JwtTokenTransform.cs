using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace ApiGateway;

public class JwtTokenTransform : ITransformProvider
{
    private readonly ILogger<JwtTokenTransform> _logger;

    public JwtTokenTransform(ILogger<JwtTokenTransform> logger)
    {
        _logger = logger;
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            if (transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var cleanToken = token.ToString().Replace("Bearer ", "");
                transformContext.ProxyRequest.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                _logger.LogDebug("Forwarded JWT token to downstream service");
            }

            return default;
        });
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }
}