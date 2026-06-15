using YugiDeck.Core.DTOs.Cards;

namespace YugiDeck.Core.DTOs.Decks;

public class DeckDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<CardDto> MainDeck { get; set; } = [];
    public List<CardDto> ExtraDeck { get; set; } = [];
    public List<CardDto> SideDeck { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
