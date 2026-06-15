namespace YugiDeck.Core.DTOs.Decks;

public class DeckDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int MainCount { get; set; }
    public int ExtraCount { get; set; }
    public int SideCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
