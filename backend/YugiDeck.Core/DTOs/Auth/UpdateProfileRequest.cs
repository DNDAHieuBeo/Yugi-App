using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required, MinLength(2), MaxLength(50)]
    public string Username { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";
}
