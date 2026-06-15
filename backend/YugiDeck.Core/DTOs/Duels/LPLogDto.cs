namespace YugiDeck.Core.DTOs.Duels;

public class LPLogDto
{
    public int Id { get; set; }
    public int PlayerNumber { get; set; }
    public int Delta { get; set; }
    public int NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}
