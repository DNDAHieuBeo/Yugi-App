using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Auth;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, MinLength(2), MaxLength(50)]
    public string Username { get; set; } = "";
}
