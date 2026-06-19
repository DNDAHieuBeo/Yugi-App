using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}
