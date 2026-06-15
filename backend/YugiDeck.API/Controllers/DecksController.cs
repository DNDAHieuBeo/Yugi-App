using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YugiDeck.Core.DTOs.Decks;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DecksController(IDeckService deckService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!;

    [HttpGet]
    public async Task<IActionResult> GetDecks()
    {
        var result = await deckService.GetDecksAsync(UserId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDeck(int id)
    {
        var result = await deckService.GetDeckByIdAsync(UserId, id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDeck(SaveDeckRequest request)
    {
        var result = await deckService.CreateDeckAsync(UserId, request);
        return CreatedAtAction(nameof(GetDeck), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> SaveDeck(int id, SaveDeckRequest request)
    {
        var result = await deckService.SaveDeckAsync(UserId, id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var deleted = await deckService.DeleteDeckAsync(UserId, id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/validate")]
    public async Task<IActionResult> ValidateDeck(int id)
    {
        var result = await deckService.ValidateDeckAsync(UserId, id);
        return Ok(result);
    }
}
