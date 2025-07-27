using System.Security.Claims;
using AuthService.Controllers.DTO.Requests;
using AuthService.Controllers.DTO.Responses;
using AuthService.Logic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var success = await _authServices.RegisterAsync(request);
        if (!success)
            return BadRequest("Пользователь с таким Email уже существует.");

        return Ok("Успешная регистрация.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authServices.LoginAsync(request);
        if (token == null)
            return Unauthorized("Неверное имя пользователя или пароль.");

        return Ok(new AuthResponse { Token = token });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetCurrentUser()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var user = await _authServices.GetUserInfoAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден.");

        return Ok(user);
    }
}