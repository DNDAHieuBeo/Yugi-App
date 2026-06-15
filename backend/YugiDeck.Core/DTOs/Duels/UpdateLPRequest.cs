using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Duels;

public class UpdateLPRequest
{
    [Required, Range(1, 2)]
    public int PlayerNumber { get; set; }

    [Required]
    public int Delta { get; set; }
}
