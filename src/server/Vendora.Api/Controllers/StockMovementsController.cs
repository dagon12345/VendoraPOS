using Microsoft.AspNetCore.Mvc;
using Vendora.Application.StockMovements;

namespace Vendora.Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/stock-movements")]
public class StockMovementsController(IStockMovementService stockMovementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockMovementDto>>> GetHistory(Guid productId, CancellationToken ct)
    {
        var history = await stockMovementService.GetHistoryAsync(productId, ct);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementDto>> Record(Guid productId, RecordStockMovementRequest request, CancellationToken ct)
    {
        try
        {
            var movement = await stockMovementService.RecordAsync(productId, request, ct);
            return movement is null ? NotFound() : CreatedAtAction(nameof(GetHistory), new { productId }, movement);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }
}
