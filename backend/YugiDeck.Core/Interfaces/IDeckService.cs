using YugiDeck.Core.DTOs.Decks;

namespace YugiDeck.Core.Interfaces;

public interface IDeckService
{
    Task<List<DeckDto>> GetDecksAsync(string userId);
    Task<DeckDetailDto?> GetDeckByIdAsync(string userId, int deckId);
    Task<DeckDto> CreateDeckAsync(string userId, SaveDeckRequest request);
    Task<DeckDetailDto?> SaveDeckAsync(string userId, int deckId, SaveDeckRequest request);
    Task<bool> DeleteDeckAsync(string userId, int deckId);
    Task<DeckValidationResult> ValidateDeckAsync(string userId, int deckId);
}
