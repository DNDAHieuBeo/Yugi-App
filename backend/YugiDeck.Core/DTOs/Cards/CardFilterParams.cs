namespace YugiDeck.Core.DTOs.Cards;

public class CardFilterParams
{
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public string? Category { get; set; }   // "Monster", "Spell", "Trap" — partial match on Type
    public string? Type { get; set; }       // exact match: "Effect Monster", "Fusion Monster", etc.
    public string? Race { get; set; }       // monster race or spell/trap subtype
    public string? Attribute { get; set; }
    public string? Archetype { get; set; }
    public int? Level { get; set; }
    public int? MinAtk { get; set; }
    public int? MaxAtk { get; set; }
    public int? MinDef { get; set; }
    public int? MaxDef { get; set; }
    public string? BanTcg { get; set; }
    public string? OrderBy { get; set; }    // "name" | "atk" | "def" | "level"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
