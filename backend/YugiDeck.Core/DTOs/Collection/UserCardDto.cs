using YugiDeck.Core.DTOs.Cards;

namespace YugiDeck.Core.DTOs.Collection;

public class UserCardDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public CardDto Card { get; set; } = null!;
}
