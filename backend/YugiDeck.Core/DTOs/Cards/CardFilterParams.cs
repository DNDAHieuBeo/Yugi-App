namespace YugiDeck.Core.DTOs.Cards;

public class CardFilterParams
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Race { get; set; }
    public string? Attribute { get; set; }
    public string? Archetype { get; set; }
    public string? BanTcg { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
