using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YugiDeck.Core.DTOs.Cards;
using YugiDeck.Core.Entities;
using YugiDeck.Core.Interfaces;
using YugiDeck.Infrastructure.Data;

namespace YugiDeck.Infrastructure.Services;

public class CardService(
    AppDbContext db,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<CardService> logger) : ICardService
{
    public async Task<PagedResult<CardDto>> GetCardsAsync(CardFilterParams filter)
    {
        var query = db.Cards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => c.Name.Contains(filter.Name));
        if (!string.IsNullOrWhiteSpace(filter.Type))
            query = query.Where(c => c.Type == filter.Type);
        if (!string.IsNullOrWhiteSpace(filter.Race))
            query = query.Where(c => c.Race == filter.Race);
        if (!string.IsNullOrWhiteSpace(filter.Attribute))
            query = query.Where(c => c.Attribute == filter.Attribute);
        if (!string.IsNullOrWhiteSpace(filter.Archetype))
            query = query.Where(c => c.Archetype == filter.Archetype);
        if (!string.IsNullOrWhiteSpace(filter.BanTcg))
            query = query.Where(c => c.BanTcg == filter.BanTcg);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync();

        return new PagedResult<CardDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<CardDto?> GetCardByIdAsync(int id)
    {
        var card = await db.Cards.FindAsync(id);
        return card is null ? null : ToDto(card);
    }

    public async Task<int> SyncFromYgoApiAsync()
    {
        var baseUrl = config["YgoApi:BaseUrl"]!;
        var client = httpClientFactory.CreateClient("YgoApi");

        logger.LogInformation("Syncing cards from YGOPRODeck...");

        var response = await client.GetAsync($"{baseUrl}cardinfo.php?misc=yes");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;
        var cardData = root.GetProperty("data");

        // Load all existing IDs in one query — avoids N+1 (12,000 FindAsync calls)
        var existingIds = await db.Cards.Select(c => c.Id).ToHashSetAsync();
        var existingCards = existingIds.Count > 0
            ? await db.Cards.ToDictionaryAsync(c => c.Id)
            : [];

        int added = 0, updated = 0;

        foreach (var item in cardData.EnumerateArray())
        {
            var id = item.GetProperty("id").GetInt32();

            var imageUrl = "";
            var imageUrlSmall = "";
            if (item.TryGetProperty("card_images", out var images) && images.GetArrayLength() > 0)
            {
                var first = images[0];
                imageUrl = first.TryGetProperty("image_url", out var img) ? img.GetString() ?? "" : "";
                imageUrlSmall = first.TryGetProperty("image_url_small", out var imgSmall) ? imgSmall.GetString() ?? "" : "";
            }

            string? banTcg = null, banOcg = null;
            if (item.TryGetProperty("banlist_info", out var banlist))
            {
                banlist.TryGetProperty("ban_tcg", out var tcg);
                banlist.TryGetProperty("ban_ocg", out var ocg);
                banTcg = tcg.ValueKind == JsonValueKind.String ? tcg.GetString() : null;
                banOcg = ocg.ValueKind == JsonValueKind.String ? ocg.GetString() : null;
            }

            var name      = item.GetProperty("name").GetString() ?? "";
            var type      = item.GetProperty("type").GetString() ?? "";
            var frameType = item.TryGetProperty("frameType", out var ft)  ? ft.GetString()  ?? "" : "";
            var desc      = item.GetProperty("desc").GetString() ?? "";
            var atk       = item.TryGetProperty("atk",       out var atkV) && atkV.ValueKind == JsonValueKind.Number ? atkV.GetInt32() : (int?)null;
            var def       = item.TryGetProperty("def",       out var defV) && defV.ValueKind == JsonValueKind.Number ? defV.GetInt32() : (int?)null;
            var level     = item.TryGetProperty("level",     out var lvlV) && lvlV.ValueKind == JsonValueKind.Number ? lvlV.GetInt32() : (int?)null;
            var race      = item.TryGetProperty("race",      out var raceV)  ? raceV.GetString()  : null;
            var attribute = item.TryGetProperty("attribute", out var attrV)  ? attrV.GetString()  : null;
            var archetype = item.TryGetProperty("archetype", out var archV)  ? archV.GetString()  : null;

            if (!existingIds.Contains(id))
            {
                db.Cards.Add(new Card
                {
                    Id = id, Name = name, Type = type, FrameType = frameType, Desc = desc,
                    Atk = atk, Def = def, Level = level, Race = race,
                    Attribute = attribute, Archetype = archetype,
                    ImageUrl = imageUrl, ImageUrlSmall = imageUrlSmall,
                    BanTcg = banTcg, BanOcg = banOcg, SyncedAt = DateTime.UtcNow
                });
                added++;
            }
            else
            {
                var existing = existingCards[id];
                existing.Name = name; existing.Type = type; existing.FrameType = frameType;
                existing.Desc = desc; existing.Atk = atk; existing.Def = def; existing.Level = level;
                existing.Race = race; existing.Attribute = attribute; existing.Archetype = archetype;
                existing.ImageUrl = imageUrl; existing.ImageUrlSmall = imageUrlSmall;
                existing.BanTcg = banTcg; existing.BanOcg = banOcg; existing.SyncedAt = DateTime.UtcNow;
                updated++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Sync complete: {Added} added, {Updated} updated.", added, updated);
        return added + updated;
    }

    private static CardDto ToDto(Card c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Type = c.Type,
        FrameType = c.FrameType,
        Desc = c.Desc,
        Atk = c.Atk,
        Def = c.Def,
        Level = c.Level,
        Race = c.Race,
        Attribute = c.Attribute,
        Archetype = c.Archetype,
        ImageUrl = c.ImageUrl,
        ImageUrlSmall = c.ImageUrlSmall,
        BanTcg = c.BanTcg,
        BanOcg = c.BanOcg
    };
}
