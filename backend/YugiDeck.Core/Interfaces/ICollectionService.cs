using YugiDeck.Core.DTOs.Collection;

namespace YugiDeck.Core.Interfaces;

public interface ICollectionService
{
    Task<List<UserCardDto>> GetCollectionAsync(string userId);
    Task<UserCardDto> AddCardAsync(string userId, AddCardRequest request);
    Task<UserCardDto?> UpdateQuantityAsync(string userId, int cardId, int quantity);
    Task<bool> RemoveCardAsync(string userId, int cardId);
}
