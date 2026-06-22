namespace YugiDeck.Core.DTOs.Cards;

public class MarketFilterParams
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Archetype { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? PriceSource { get; set; } = "cardmarket"; // cardmarket | tcgplayer | ebay | amazon
    public string? OrderBy { get; set; } = "price_desc";     // price_desc | price_asc | name
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
}
