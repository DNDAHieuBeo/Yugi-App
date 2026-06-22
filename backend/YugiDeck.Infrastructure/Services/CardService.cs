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
        {
            var name = filter.Name.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(name));
        }
        if (!string.IsNullOrWhiteSpace(filter.Desc))
        {
            var desc = filter.Desc.ToLower();
            query = query.Where(c => c.Desc.ToLower().Contains(desc));
        }
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.ToLower();
            query = query.Where(c => c.Type.ToLower().Contains(category));
        }
        if (!string.IsNullOrWhiteSpace(filter.Type))
            query = query.Where(c => c.Type.ToLower() == filter.Type.ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Race))
            query = query.Where(c => c.Race != null && c.Race.ToLower() == filter.Race.ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Attribute))
            query = query.Where(c => c.Attribute != null && c.Attribute.ToLower() == filter.Attribute.ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Archetype))
            query = query.Where(c => c.Archetype != null && c.Archetype.ToLower() == filter.Archetype.ToLower());
        if (filter.Level.HasValue)
            query = query.Where(c => c.Level == filter.Level);
        if (filter.MinAtk.HasValue)
            query = query.Where(c => c.Atk >= filter.MinAtk);
        if (filter.MaxAtk.HasValue)
            query = query.Where(c => c.Atk <= filter.MaxAtk);
        if (filter.MinDef.HasValue)
            query = query.Where(c => c.Def >= filter.MinDef);
        if (filter.MaxDef.HasValue)
            query = query.Where(c => c.Def <= filter.MaxDef);
        if (!string.IsNullOrWhiteSpace(filter.BanTcg))
            query = query.Where(c => c.BanTcg == filter.BanTcg);

        var total = await query.CountAsync();

        IQueryable<Card> ordered = filter.OrderBy switch
        {
            "atk"    => query.OrderByDescending(c => c.Atk),
            "def"    => query.OrderByDescending(c => c.Def),
            "level"  => query.OrderByDescending(c => c.Level),
            "name"   => query.OrderBy(c => c.Name),
            "oldest" => query.OrderBy(c => c.Id),
            _        => query.OrderByDescending(c => c.Id)  // "newest" + default
        };

        var items = await ordered
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

    public async Task<PagedResult<CardDto>> GetMarketCardsAsync(MarketFilterParams filter)
    {
        // Only cards that have a price for the selected source
        var query = filter.PriceSource switch
        {
            "tcgplayer" => db.Cards.Where(c => c.TcgplayerPrice != null && c.TcgplayerPrice > 0),
            "ebay"      => db.Cards.Where(c => c.EbayPrice != null && c.EbayPrice > 0),
            "amazon"    => db.Cards.Where(c => c.AmazonPrice != null && c.AmazonPrice > 0),
            _           => db.Cards.Where(c => c.CardmarketPrice != null && c.CardmarketPrice > 0),
        };

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(name));
        }
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var cat = filter.Category.ToLower();
            query = query.Where(c => c.Type.ToLower().Contains(cat));
        }
        if (!string.IsNullOrWhiteSpace(filter.Archetype))
        {
            var arch = filter.Archetype.ToLower();
            query = query.Where(c => c.Archetype != null && c.Archetype.ToLower() == arch);
        }

        // Price range filter on selected source
        if (filter.MinPrice.HasValue)
            query = filter.PriceSource switch
            {
                "tcgplayer" => query.Where(c => c.TcgplayerPrice >= filter.MinPrice),
                "ebay"      => query.Where(c => c.EbayPrice >= filter.MinPrice),
                "amazon"    => query.Where(c => c.AmazonPrice >= filter.MinPrice),
                _           => query.Where(c => c.CardmarketPrice >= filter.MinPrice),
            };
        if (filter.MaxPrice.HasValue)
            query = filter.PriceSource switch
            {
                "tcgplayer" => query.Where(c => c.TcgplayerPrice <= filter.MaxPrice),
                "ebay"      => query.Where(c => c.EbayPrice <= filter.MaxPrice),
                "amazon"    => query.Where(c => c.AmazonPrice <= filter.MaxPrice),
                _           => query.Where(c => c.CardmarketPrice <= filter.MaxPrice),
            };

        var total = await query.CountAsync();

        IQueryable<Card> ordered = (filter.PriceSource, filter.OrderBy) switch
        {
            ("tcgplayer", "price_asc") => query.OrderBy(c => c.TcgplayerPrice),
            ("tcgplayer", "name")      => query.OrderBy(c => c.Name),
            ("tcgplayer", _)           => query.OrderByDescending(c => c.TcgplayerPrice),
            ("ebay",      "price_asc") => query.OrderBy(c => c.EbayPrice),
            ("ebay",      "name")      => query.OrderBy(c => c.Name),
            ("ebay",      _)           => query.OrderByDescending(c => c.EbayPrice),
            ("amazon",    "price_asc") => query.OrderBy(c => c.AmazonPrice),
            ("amazon",    "name")      => query.OrderBy(c => c.Name),
            ("amazon",    _)           => query.OrderByDescending(c => c.AmazonPrice),
            (_,           "price_asc") => query.OrderBy(c => c.CardmarketPrice),
            (_,           "name")      => query.OrderBy(c => c.Name),
            _                          => query.OrderByDescending(c => c.CardmarketPrice),
        };

        var items = await ordered
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

            decimal? cardmarketPrice = null, tcgplayerPrice = null, ebayPrice = null, amazonPrice = null;
            if (item.TryGetProperty("card_prices", out var prices) && prices.GetArrayLength() > 0)
            {
                var p = prices[0];
                cardmarketPrice = ParsePrice(p, "cardmarket_price");
                tcgplayerPrice  = ParsePrice(p, "tcgplayer_price");
                ebayPrice       = ParsePrice(p, "ebay_price");
                amazonPrice     = ParsePrice(p, "amazon_price");
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
                    BanTcg = banTcg, BanOcg = banOcg,
                    CardmarketPrice = cardmarketPrice, TcgplayerPrice = tcgplayerPrice,
                    EbayPrice = ebayPrice, AmazonPrice = amazonPrice,
                    SyncedAt = DateTime.UtcNow
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
                existing.BanTcg = banTcg; existing.BanOcg = banOcg;
                existing.CardmarketPrice = cardmarketPrice; existing.TcgplayerPrice = tcgplayerPrice;
                existing.EbayPrice = ebayPrice; existing.AmazonPrice = amazonPrice;
                existing.SyncedAt = DateTime.UtcNow;
                updated++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Sync complete: {Added} added, {Updated} updated.", added, updated);
        return added + updated;
    }

    private static decimal? ParsePrice(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var val)) return null;
        var str = val.ValueKind == JsonValueKind.String ? val.GetString() : null;
        if (string.IsNullOrEmpty(str)) return null;
        return decimal.TryParse(str, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) && d > 0 ? d : null;
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
        BanOcg = c.BanOcg,
        CardmarketPrice = c.CardmarketPrice,
        TcgplayerPrice  = c.TcgplayerPrice,
        EbayPrice       = c.EbayPrice,
        AmazonPrice     = c.AmazonPrice,
    };
}
