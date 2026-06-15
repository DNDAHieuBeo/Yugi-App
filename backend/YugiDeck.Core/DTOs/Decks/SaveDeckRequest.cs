using System.ComponentModel.DataAnnotations;

namespace YugiDeck.Core.DTOs.Decks;

public class SaveDeckRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<int> MainDeck { get; set; } = [];
    public List<int> ExtraDeck { get; set; } = [];
    public List<int> SideDeck { get; set; } = [];
}
