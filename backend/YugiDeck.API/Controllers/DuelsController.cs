using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YugiDeck.Core.DTOs.Duels;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuelsController(IDuelService duelService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!;

    [HttpGet]
    public async Task<IActionResult> GetDuels()
    {
        var result = await duelService.GetDuelsAsync(UserId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDuel(int id)
    {
        var result = await duelService.GetDuelByIdAsync(UserId, id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDuel(CreateDuelRequest request)
    {
        var result = await duelService.CreateDuelAsync(UserId, request);
        return CreatedAtAction(nameof(GetDuel), new { id = result.Id }, result);
    }

    [HttpPost("{id:int}/lp")]
    public async Task<IActionResult> UpdateLP(int id, UpdateLPRequest request)
    {
        var result = await duelService.UpdateLPAsync(UserId, id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var result = await duelService.GetHistoryAsync(UserId, id);
        return Ok(result);
    }

    [HttpPost("{id:int}/end")]
    public async Task<IActionResult> EndDuel(int id)
    {
        var result = await duelService.EndDuelAsync(UserId, id);
        return result is null ? NotFound() : Ok(result);
    }
}
