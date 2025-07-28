using System.Security.Cryptography;
using System.Text;
using AuthService.Controllers.DTO.Requests;
using AuthService.Data.Entities;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Logic.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthService.Tests;

public class AuthServicesTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<AuthServices>> _loggerMock = new();
    private readonly AuthServices _sut;
    private readonly JwtTokenGenerator _jwtGenerator;

    public AuthServicesTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "MySuperSecretKey1234567890_ExtraChars12345"},
            {"Jwt:Issuer", "MyAuthService"},
            {"Jwt:Audience", "MyAuthServiceUsers"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtLoggerMock = new Mock<ILogger<JwtTokenGenerator>>();
        _jwtGenerator = new JwtTokenGenerator(configuration, jwtLoggerMock.Object);

        _sut = new AuthServices(
            _userRepositoryMock.Object,
            _jwtGenerator,
            _loggerMock.Object
        );
    }

    private static User CreateTestUser(int id = 1, string email = "test@example.com") => new()
    {
        Id = id,
        Username = "testuser",
        Email = email,
        PasswordHash = "hashedpassword",
        CreatedAt = DateTime.UtcNow
    };

    private static string ComputeSha256Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsTrue()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "password123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        var result = await _sut.RegisterAsync(request);

        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Email == request.Email &&
            u.Username == request.Username)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ExistingUser_ReturnsFalse()
    {
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "password123"
        };

        var existingUser = CreateTestUser(email: request.Email);
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var result = await _sut.RegisterAsync(request);

        result.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var password = "correctpassword";
        var request = new LoginRequest
        {
            Email = "valid@example.com",
            Password = password
        };

        var user = CreateTestUser(email: request.Email);
        user.PasswordHash = ComputeSha256Hash(password);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("ey");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        var request = new LoginRequest
        {
            Email = "valid@example.com",
            Password = "wrongpassword"
        };

        var user = CreateTestUser(email: request.Email);
        user.PasswordHash = ComputeSha256Hash("correctpassword");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        var request = new LoginRequest
        {
            Email = "missing@example.com",
            Password = "any"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserInfoAsync_UserExists_ReturnsUserInfo()
    {
        var user = CreateTestUser();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var result = await _sut.GetUserInfoAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetUserInfoAsync_UserNotFound_ReturnsNull()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserInfoAsync(999);

        result.Should().BeNull();
    }
}