using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Logic.Services;

public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenGenerator> _logger;

    public JwtTokenGenerator(IConfiguration configuration, ILogger<JwtTokenGenerator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(int userId, string username)
    {
        var secret = _configuration["Jwt:Secret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            _logger.LogError("Не удалось сгенерировать токен: отсутствуют настройки JWT (Secret, Issuer, Audience)");
            throw new InvalidOperationException("JWT configuration is invalid");
        }
        
        var key = Encoding.UTF8.GetBytes(secret);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = issuer,
            Audience = audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Сгенерирован JWT токен для пользователя {UserId}", userId);
        return tokenString;
    }
}