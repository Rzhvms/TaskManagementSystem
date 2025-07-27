using System.Net.Http.Headers;
using TaskService.Controllers.DTO.Requests;
using TaskService.Logic.Services.Interfaces;

namespace TaskService.Logic.Services;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string NotificationsEndpoint = "api/notifications";

    public NotificationClient(
        HttpClient httpClient, 
        ILogger<NotificationClient> logger, 
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendNotificationAsync(CreateNotificationRequest request)
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, NotificationsEndpoint)
            {
                Content = JsonContent.Create(request)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Не удалось отправить уведомление. Код: {StatusCode}. Ответ: {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке уведомления");
        }
    }
}