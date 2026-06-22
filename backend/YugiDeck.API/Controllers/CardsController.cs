using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YugiDeck.Core.DTOs.Cards;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController(ICardService cardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCards([FromQuery] CardFilterParams filter)
    {
        var result = await cardService.GetCardsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCard(int id)
    {
        var card = await cardService.GetCardByIdAsync(id);
        return card is null ? NotFound() : Ok(card);
    }

    [HttpGet("market")]
    public async Task<IActionResult> GetMarket([FromQuery] MarketFilterParams filter)
    {
        var result = await cardService.GetMarketCardsAsync(filter);
        return Ok(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var count = await cardService.SyncFromYgoApiAsync();
        return Ok(new { synced = count });
    }
}
