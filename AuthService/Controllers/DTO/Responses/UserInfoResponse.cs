namespace AuthService.Controllers.DTO.Responses;

public class UserInfoResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}