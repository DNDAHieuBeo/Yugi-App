using YugiDeck.Core.DTOs.Duels;

namespace YugiDeck.Core.Interfaces;

public interface IDuelService
{
    Task<List<DuelDto>> GetDuelsAsync(string userId);
    Task<DuelDto?> GetDuelByIdAsync(string userId, int duelId);
    Task<DuelDto> CreateDuelAsync(string userId, CreateDuelRequest request);
    Task<DuelDto?> UpdateLPAsync(string userId, int duelId, UpdateLPRequest request);
    Task<List<LPLogDto>> GetHistoryAsync(string userId, int duelId);
    Task<DuelDto?> EndDuelAsync(string userId, int duelId);
}
