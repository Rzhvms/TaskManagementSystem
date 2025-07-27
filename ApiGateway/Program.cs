using ApiGateway;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Http Logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

// Memory Cache
builder.Services.AddMemoryCache();

// Circuit Breaker
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3, 
        durationOfBreak: TimeSpan.FromSeconds(30));

builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter>(_ =>
    new PolicyHttpMessageHandlerBuilderFilter(circuitBreakerPolicy));

// YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<JwtTokenTransform>();

var app = builder.Build();

// Middleware
app.UseHttpLogging();
app.UseWebSockets();
app.UseHttpsRedirection();

app.MapReverseProxy();

app.Run();