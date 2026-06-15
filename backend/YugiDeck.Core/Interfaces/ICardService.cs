using YugiDeck.Core.DTOs.Cards;

namespace YugiDeck.Core.Interfaces;

public interface ICardService
{
    Task<PagedResult<CardDto>> GetCardsAsync(CardFilterParams filter);
    Task<CardDto?> GetCardByIdAsync(int id);
    Task<int> SyncFromYgoApiAsync();
}
