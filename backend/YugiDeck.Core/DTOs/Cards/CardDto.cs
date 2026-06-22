namespace YugiDeck.Core.DTOs.Cards;

public class CardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string FrameType { get; set; } = "";
    public string Desc { get; set; } = "";
    public int? Atk { get; set; }
    public int? Def { get; set; }
    public int? Level { get; set; }
    public string? Race { get; set; }
    public string? Attribute { get; set; }
    public string? Archetype { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrlSmall { get; set; }
    public string? BanTcg { get; set; }
    public string? BanOcg { get; set; }
    public decimal? CardmarketPrice { get; set; }
    public decimal? TcgplayerPrice { get; set; }
    public decimal? EbayPrice { get; set; }
    public decimal? AmazonPrice { get; set; }
}
