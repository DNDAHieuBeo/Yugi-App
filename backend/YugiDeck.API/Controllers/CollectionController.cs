using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YugiDeck.Core.DTOs.Collection;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollectionController(ICollectionService collectionService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!;

    [HttpGet]
    public async Task<IActionResult> GetCollection()
    {
        var result = await collectionService.GetCollectionAsync(UserId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddCard(AddCardRequest request)
    {
        try
        {
            var result = await collectionService.AddCardAsync(UserId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{cardId:int}/quantity")]
    public async Task<IActionResult> UpdateQuantity(int cardId, UpdateQuantityRequest request)
    {
        var result = await collectionService.UpdateQuantityAsync(UserId, cardId, request.Quantity);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpDelete("{cardId:int}")]
    public async Task<IActionResult> RemoveCard(int cardId)
    {
        var removed = await collectionService.RemoveCardAsync(UserId, cardId);
        return removed ? NoContent() : NotFound();
    }
}
