namespace YugiDeck.Core.DTOs.Duels;

public class DuelDto
{
    public int Id { get; set; }
    public string Player1Name { get; set; } = "";
    public string Player2Name { get; set; } = "";
    public int Player1LP { get; set; }
    public int Player2LP { get; set; }
    public string Status { get; set; } = "";
    public string? WinnerId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
