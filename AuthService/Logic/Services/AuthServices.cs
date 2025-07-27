using System.Security.Cryptography;
using System.Text;
using AuthService.Controllers.DTO.Requests;
using AuthService.Controllers.DTO.Responses;
using AuthService.Data.Entities;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Logic.Services.Interfaces;

namespace AuthService.Logic.Services;

public class AuthServices : IAuthServices
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenGenerator _jwtGenerator;
    private readonly ILogger<AuthServices> _logger;

    public AuthServices(
        IUserRepository userRepository, 
        JwtTokenGenerator jwtGenerator,
        ILogger<AuthServices> logger)
    {
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        
        if (existingUser != null)
        {
            _logger.LogWarning("Регистрация не удалась: пользователь с email '{Email}' уже существует", request.Email);
            return false;
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        _logger.LogInformation("Пользователь зарегистрирован: {Email}", request.Email);
        return true;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null)
        {
            _logger.LogWarning("Попытка входа с несуществующим email: {Email}", request.Email);
            return null;
        }
        
        if (user.PasswordHash != HashPassword(request.Password))
        {
            _logger.LogWarning("Неверный пароль для пользователя: {Email}", request.Email);
            return null;
        }

        var token = _jwtGenerator.GenerateToken(user.Id, user.Username);
        _logger.LogInformation("Пользователь вошёл в систему: {Email}", request.Email);
        return token;
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Не найден пользователь с ID: {UserId}", userId);
            return null;
        }

        _logger.LogInformation("Информация о пользователе возвращена: {UserId}", userId);
        return MapToResponse(user);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private UserInfoResponse MapToResponse(User user)
    {
        return new UserInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
