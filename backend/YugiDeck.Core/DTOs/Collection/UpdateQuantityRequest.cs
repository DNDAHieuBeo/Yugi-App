using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Collection;

public class UpdateQuantityRequest
{
    [Range(0, 99)]
    public int Quantity { get; set; }
}
