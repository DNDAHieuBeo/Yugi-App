using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Collection;

public class AddCardRequest
{
    [Required]
    public int CardId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;
}
