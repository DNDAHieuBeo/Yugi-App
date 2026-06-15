using Microsoft.EntityFrameworkCore;
using YugiDeck.Core.DTOs.Cards;
using YugiDeck.Core.DTOs.Collection;
using YugiDeck.Core.Entities;
using YugiDeck.Core.Interfaces;
using YugiDeck.Infrastructure.Data;

namespace YugiDeck.Infrastructure.Services;

public class CollectionService(AppDbContext db) : ICollectionService
{
    public async Task<List<UserCardDto>> GetCollectionAsync(string userId)
    {
        return await db.UserCards
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Card)
            .OrderBy(uc => uc.Card.Name)
            .Select(uc => ToDto(uc))
            .ToListAsync();
    }

    public async Task<UserCardDto> AddCardAsync(string userId, AddCardRequest request)
    {
        var card = await db.Cards.FindAsync(request.CardId)
            ?? throw new KeyNotFoundException($"Card {request.CardId} not found.");

        var existing = await db.UserCards
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CardId == request.CardId);

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            await db.SaveChangesAsync();
            existing.Card = card;
            return ToDto(existing);
        }

        var userCard = new UserCard
        {
            UserId = userId,
            CardId = request.CardId,
            Quantity = request.Quantity,
            Card = card
        };
        db.UserCards.Add(userCard);
        await db.SaveChangesAsync();
        return ToDto(userCard);
    }

    public async Task<UserCardDto?> UpdateQuantityAsync(string userId, int cardId, int quantity)
    {
        var userCard = await db.UserCards
            .Include(uc => uc.Card)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CardId == cardId);

        if (userCard is null) return null;

        if (quantity <= 0)
        {
            db.UserCards.Remove(userCard);
            await db.SaveChangesAsync();
            return null;
        }

        userCard.Quantity = quantity;
        await db.SaveChangesAsync();
        return ToDto(userCard);
    }

    public async Task<bool> RemoveCardAsync(string userId, int cardId)
    {
        var userCard = await db.UserCards
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CardId == cardId);

        if (userCard is null) return false;

        db.UserCards.Remove(userCard);
        await db.SaveChangesAsync();
        return true;
    }

    private static UserCardDto ToDto(UserCard uc) => new()
    {
        Id = uc.Id,
        Quantity = uc.Quantity,
        Card = new CardDto
        {
            Id = uc.Card.Id,
            Name = uc.Card.Name,
            Type = uc.Card.Type,
            FrameType = uc.Card.FrameType,
            Desc = uc.Card.Desc,
            Atk = uc.Card.Atk,
            Def = uc.Card.Def,
            Level = uc.Card.Level,
            Race = uc.Card.Race,
            Attribute = uc.Card.Attribute,
            Archetype = uc.Card.Archetype,
            ImageUrl = uc.Card.ImageUrl,
            ImageUrlSmall = uc.Card.ImageUrlSmall,
            BanTcg = uc.Card.BanTcg,
            BanOcg = uc.Card.BanOcg
        }
    };
}
