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

        int synced = 0;

        foreach (var item in cardData.EnumerateArray())
        {
            var id = item.GetProperty("id").GetInt32();
            var existing = await db.Cards.FindAsync(id);

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

            if (existing is null)
            {
                db.Cards.Add(new Card
                {
                    Id = id,
                    Name = item.GetProperty("name").GetString() ?? "",
                    Type = item.GetProperty("type").GetString() ?? "",
                    FrameType = item.TryGetProperty("frameType", out var ft) ? ft.GetString() ?? "" : "",
                    Desc = item.GetProperty("desc").GetString() ?? "",
                    Atk = item.TryGetProperty("atk", out var atk) && atk.ValueKind == JsonValueKind.Number ? atk.GetInt32() : null,
                    Def = item.TryGetProperty("def", out var def) && def.ValueKind == JsonValueKind.Number ? def.GetInt32() : null,
                    Level = item.TryGetProperty("level", out var lvl) && lvl.ValueKind == JsonValueKind.Number ? lvl.GetInt32() : null,
                    Race = item.TryGetProperty("race", out var race) ? race.GetString() : null,
                    Attribute = item.TryGetProperty("attribute", out var attr) ? attr.GetString() : null,
                    Archetype = item.TryGetProperty("archetype", out var arch) ? arch.GetString() : null,
                    ImageUrl = imageUrl,
                    ImageUrlSmall = imageUrlSmall,
                    BanTcg = banTcg,
                    BanOcg = banOcg,
                    SyncedAt = DateTime.UtcNow
                });
                synced++;
            }
            else
            {
                existing.Name = item.GetProperty("name").GetString() ?? "";
                existing.Type = item.GetProperty("type").GetString() ?? "";
                existing.FrameType = item.TryGetProperty("frameType", out var ft2) ? ft2.GetString() ?? "" : "";
                existing.Desc = item.GetProperty("desc").GetString() ?? "";
                existing.Atk = item.TryGetProperty("atk", out var atk2) && atk2.ValueKind == JsonValueKind.Number ? atk2.GetInt32() : null;
                existing.Def = item.TryGetProperty("def", out var def2) && def2.ValueKind == JsonValueKind.Number ? def2.GetInt32() : null;
                existing.Level = item.TryGetProperty("level", out var lvl2) && lvl2.ValueKind == JsonValueKind.Number ? lvl2.GetInt32() : null;
                existing.Race = item.TryGetProperty("race", out var race2) ? race2.GetString() : null;
                existing.Attribute = item.TryGetProperty("attribute", out var attr2) ? attr2.GetString() : null;
                existing.Archetype = item.TryGetProperty("archetype", out var arch2) ? arch2.GetString() : null;
                existing.ImageUrl = imageUrl;
                existing.ImageUrlSmall = imageUrlSmall;
                existing.BanTcg = banTcg;
                existing.BanOcg = banOcg;
                existing.SyncedAt = DateTime.UtcNow;
                synced++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Synced {Count} cards.", synced);
        return synced;
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
