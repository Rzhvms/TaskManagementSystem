using Microsoft.Extensions.Http;
using Polly;

namespace ApiGateway;

public class PolicyHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public PolicyHttpMessageHandlerBuilderFilter(IAsyncPolicy<HttpResponseMessage> policy)
    {
        _policy = policy;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        if (next == null) throw new ArgumentNullException(nameof(next));

        return builder =>
        {
            next(builder);
            builder.AdditionalHandlers.Add(new PolicyHttpMessageHandler(_policy));
        };
    }
}
