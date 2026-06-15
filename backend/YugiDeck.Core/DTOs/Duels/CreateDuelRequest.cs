using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Duels;

public class CreateDuelRequest
{
    [Required, MinLength(1), MaxLength(50)]
    public string Player1Name { get; set; } = "";

    [Required, MinLength(1), MaxLength(50)]
    public string Player2Name { get; set; } = "";
}
