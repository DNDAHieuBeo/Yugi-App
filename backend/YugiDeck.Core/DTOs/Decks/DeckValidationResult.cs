namespace YugiDeck.Core.DTOs.Decks;

public class DeckValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
}
