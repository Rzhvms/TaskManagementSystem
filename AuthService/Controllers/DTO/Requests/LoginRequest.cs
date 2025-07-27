using System.ComponentModel.DataAnnotations;

namespace AuthService.Controllers.DTO.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Email - обязательное поле.")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Password - обязательное поле.")]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
    public string Password { get; set; } = null!;
}