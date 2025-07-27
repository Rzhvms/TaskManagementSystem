using AuthService.Controllers.DTO.Requests;
using AuthService.Controllers.DTO.Responses;

namespace AuthService.Logic.Services.Interfaces;

public interface IAuthServices
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
    Task<UserInfoResponse?> GetUserInfoAsync(int userId);
}