using Microsoft.EntityFrameworkCore;
using YugiDeck.Core.DTOs.Cards;
using YugiDeck.Core.DTOs.Decks;
using YugiDeck.Core.Entities;
using YugiDeck.Core.Interfaces;
using YugiDeck.Infrastructure.Data;

namespace YugiDeck.Infrastructure.Services;

public class DeckService(AppDbContext db) : IDeckService
{
    private static readonly HashSet<string> ExtraTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fusion Monster", "Synchro Monster", "XYZ Monster", "Link Monster",
        "Fusion/Effect Monster", "Synchro/Effect Monster", "Synchro/Tuner Monster",
        "XYZ/Effect Monster", "Link/Effect Monster"
    };

    public async Task<List<DeckDto>> GetDecksAsync(string userId)
    {
        return await db.Decks
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DeckDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                MainCount = d.DeckCards.Count(dc => dc.Section == "main"),
                ExtraCount = d.DeckCards.Count(dc => dc.Section == "extra"),
                SideCount = d.DeckCards.Count(dc => dc.Section == "side"),
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<DeckDetailDto?> GetDeckByIdAsync(string userId, int deckId)
    {
        var deck = await db.Decks
            .Include(d => d.DeckCards)
            .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

        return deck is null ? null : ToDetailDto(deck);
    }

    public async Task<DeckDto> CreateDeckAsync(string userId, SaveDeckRequest request)
    {
        var deck = new Deck
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        await ApplyDeckCardsAsync(deck, request);

        return new DeckDto
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description,
            MainCount = request.MainDeck.Count,
            ExtraCount = request.ExtraDeck.Count,
            SideCount = request.SideDeck.Count,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task<DeckDetailDto?> SaveDeckAsync(string userId, int deckId, SaveDeckRequest request)
    {
        var deck = await db.Decks
            .Include(d => d.DeckCards)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

        if (deck is null) return null;

        deck.Name = request.Name;
        deck.Description = request.Description;
        deck.UpdatedAt = DateTime.UtcNow;

        db.DeckCards.RemoveRange(deck.DeckCards);
        await db.SaveChangesAsync();

        await ApplyDeckCardsAsync(deck, request);

        return await GetDeckByIdAsync(userId, deckId);
    }

    public async Task<bool> DeleteDeckAsync(string userId, int deckId)
    {
        var deck = await db.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);
        if (deck is null) return false;

        db.Decks.Remove(deck);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<DeckValidationResult> ValidateDeckAsync(string userId, int deckId)
    {
        var deck = await db.Decks
            .Include(d => d.DeckCards)
            .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

        if (deck is null)
            return new DeckValidationResult { IsValid = false, Errors = ["Deck not found."] };

        var errors = new List<string>();
        var main = deck.DeckCards.Where(dc => dc.Section == "main").ToList();
        var extra = deck.DeckCards.Where(dc => dc.Section == "extra").ToList();
        var side = deck.DeckCards.Where(dc => dc.Section == "side").ToList();

        if (main.Count < 40) errors.Add($"Main deck has {main.Count} cards (minimum 40).");
        if (main.Count > 60) errors.Add($"Main deck has {main.Count} cards (maximum 60).");
        if (extra.Count > 15) errors.Add($"Extra deck has {extra.Count} cards (maximum 15).");
        if (side.Count > 15) errors.Add($"Side deck has {side.Count} cards (maximum 15).");

        var allCards = main.Concat(side).GroupBy(dc => dc.CardId);
        foreach (var group in allCards)
        {
            var card = group.First().Card;
            var count = group.Count();
            var ban = card.BanTcg?.ToLower();

            if (ban == "banned" && count > 0)
                errors.Add($"\"{card.Name}\" is Banned (0 copies allowed).");
            else if (ban == "limited" && count > 1)
                errors.Add($"\"{card.Name}\" is Limited (max 1 copy).");
            else if (ban == "semi-limited" && count > 2)
                errors.Add($"\"{card.Name}\" is Semi-Limited (max 2 copies).");
            else if (count > 3)
                errors.Add($"\"{card.Name}\" has {count} copies (max 3).");
        }

        return new DeckValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    private async Task ApplyDeckCardsAsync(Deck deck, SaveDeckRequest request)
    {
        var allIds = request.MainDeck.Concat(request.ExtraDeck).Concat(request.SideDeck).Distinct();
        var cards = await db.Cards.Where(c => allIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

        var deckCards = new List<DeckCard>();
        deckCards.AddRange(request.MainDeck.Select(id => new DeckCard { DeckId = deck.Id, CardId = id, Section = "main" }));
        deckCards.AddRange(request.ExtraDeck.Select(id => new DeckCard { DeckId = deck.Id, CardId = id, Section = "extra" }));
        deckCards.AddRange(request.SideDeck.Select(id => new DeckCard { DeckId = deck.Id, CardId = id, Section = "side" }));

        db.DeckCards.AddRange(deckCards);
        await db.SaveChangesAsync();
    }

    private static DeckDetailDto ToDetailDto(Deck deck) => new()
    {
        Id = deck.Id,
        Name = deck.Name,
        Description = deck.Description,
        MainDeck = deck.DeckCards.Where(dc => dc.Section == "main").Select(dc => CardToDto(dc.Card)).ToList(),
        ExtraDeck = deck.DeckCards.Where(dc => dc.Section == "extra").Select(dc => CardToDto(dc.Card)).ToList(),
        SideDeck = deck.DeckCards.Where(dc => dc.Section == "side").Select(dc => CardToDto(dc.Card)).ToList(),
        CreatedAt = deck.CreatedAt,
        UpdatedAt = deck.UpdatedAt
    };

    private static CardDto CardToDto(Card c) => new()
    {
        Id = c.Id, Name = c.Name, Type = c.Type, FrameType = c.FrameType, Desc = c.Desc,
        Atk = c.Atk, Def = c.Def, Level = c.Level, Race = c.Race, Attribute = c.Attribute,
        Archetype = c.Archetype, ImageUrl = c.ImageUrl, ImageUrlSmall = c.ImageUrlSmall,
        BanTcg = c.BanTcg, BanOcg = c.BanOcg
    };
}
