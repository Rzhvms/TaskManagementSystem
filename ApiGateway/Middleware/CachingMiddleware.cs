using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace ApiGateway.Middleware;

public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingMiddleware> _logger;

    public CachingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<CachingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        var cacheKey = GenerateCacheKey(context.Request);

        if (_cache.TryGetValue(cacheKey, out string cachedResponse))
        {
            _logger.LogInformation("Ответ получен из кэша {CacheKey}", cacheKey);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        _logger.LogInformation("Промах по кэшу {CacheKey}", cacheKey);

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        if (context.Response.StatusCode == 200)
        {
            _cache.Set(cacheKey, responseBody, TimeSpan.FromMinutes(5));
        }

        await memoryStream.CopyToAsync(originalBodyStream);
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(request.Path.ToString().ToLower());

        if (request.Query.Any())
        {
            foreach (var (key, value) in request.Query.OrderBy(k => k.Key))
            {
                sb.Append($"|{key}:{value}");
            }
        }

        return sb.ToString();
    }
}