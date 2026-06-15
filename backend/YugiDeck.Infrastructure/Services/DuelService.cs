using Microsoft.EntityFrameworkCore;
using YugiDeck.Core.DTOs.Duels;
using YugiDeck.Core.Entities;
using YugiDeck.Core.Interfaces;
using YugiDeck.Infrastructure.Data;

namespace YugiDeck.Infrastructure.Services;

public class DuelService(AppDbContext db) : IDuelService
{
    public async Task<List<DuelDto>> GetDuelsAsync(string userId)
    {
        return await db.Duels
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.StartedAt)
            .Select(d => ToDto(d))
            .ToListAsync();
    }

    public async Task<DuelDto?> GetDuelByIdAsync(string userId, int duelId)
    {
        var duel = await db.Duels.FirstOrDefaultAsync(d => d.Id == duelId && d.UserId == userId);
        return duel is null ? null : ToDto(duel);
    }

    public async Task<DuelDto> CreateDuelAsync(string userId, CreateDuelRequest request)
    {
        var duel = new Duel
        {
            UserId = userId,
            Player1Name = request.Player1Name,
            Player2Name = request.Player2Name,
            Player1LP = 8000,
            Player2LP = 8000,
            Status = "active",
            StartedAt = DateTime.UtcNow
        };
        db.Duels.Add(duel);
        await db.SaveChangesAsync();
        return ToDto(duel);
    }

    public async Task<DuelDto?> UpdateLPAsync(string userId, int duelId, UpdateLPRequest request)
    {
        var duel = await db.Duels.FirstOrDefaultAsync(d => d.Id == duelId && d.UserId == userId);
        if (duel is null || duel.Status == "ended") return null;

        int newValue;
        if (request.PlayerNumber == 1)
        {
            newValue = Math.Max(0, duel.Player1LP + request.Delta);
            duel.Player1LP = newValue;
        }
        else
        {
            newValue = Math.Max(0, duel.Player2LP + request.Delta);
            duel.Player2LP = newValue;
        }

        db.LPLogs.Add(new LPLog
        {
            DuelId = duelId,
            PlayerNumber = request.PlayerNumber,
            Delta = request.Delta,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow
        });

        if (duel.Player1LP == 0 || duel.Player2LP == 0)
        {
            duel.Status = "ended";
            duel.EndedAt = DateTime.UtcNow;
            duel.WinnerId = duel.Player1LP > 0 ? "player1" : "player2";
        }

        await db.SaveChangesAsync();
        return ToDto(duel);
    }

    public async Task<List<LPLogDto>> GetHistoryAsync(string userId, int duelId)
    {
        var duelExists = await db.Duels.AnyAsync(d => d.Id == duelId && d.UserId == userId);
        if (!duelExists) return [];

        return await db.LPLogs
            .Where(l => l.DuelId == duelId)
            .OrderBy(l => l.Timestamp)
            .Select(l => new LPLogDto
            {
                Id = l.Id,
                PlayerNumber = l.PlayerNumber,
                Delta = l.Delta,
                NewValue = l.NewValue,
                Timestamp = l.Timestamp
            })
            .ToListAsync();
    }

    public async Task<DuelDto?> EndDuelAsync(string userId, int duelId)
    {
        var duel = await db.Duels.FirstOrDefaultAsync(d => d.Id == duelId && d.UserId == userId);
        if (duel is null || duel.Status == "ended") return null;

        duel.Status = "ended";
        duel.EndedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(duel);
    }

    private static DuelDto ToDto(Duel d) => new()
    {
        Id = d.Id,
        Player1Name = d.Player1Name,
        Player2Name = d.Player2Name,
        Player1LP = d.Player1LP,
        Player2LP = d.Player2LP,
        Status = d.Status,
        WinnerId = d.WinnerId,
        StartedAt = d.StartedAt,
        EndedAt = d.EndedAt
    };
}
