using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = "";
}
