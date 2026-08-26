using Microsoft.AspNetCore.Mvc;
using Vendora.Application.Sales;

namespace Vendora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetAll(CancellationToken ct) =>
        Ok(await saleService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetById(Guid id, CancellationToken ct)
    {
        var sale = await saleService.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(CreateSaleRequest request, CancellationToken ct)
    {
        try
        {
            var sale = await saleService.CreateSaleAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/void")]
    public async Task<ActionResult<SaleDto>> Void(Guid id, VoidSaleRequest request, CancellationToken ct)
    {
        try
        {
            var sale = await saleService.VoidAsync(id, request, ct);
            return sale is null ? NotFound() : Ok(sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<SaleDto>> Restore(Guid id, CancellationToken ct)
    {
        try
        {
            var sale = await saleService.RestoreAsync(id, ct);
            return sale is null ? NotFound() : Ok(sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/void-line")]
    public async Task<ActionResult<SaleDto>> VoidLine(Guid id, VoidLineRequest request, CancellationToken ct)
    {
        try
        {
            var sale = await saleService.VoidLineAsync(id, request, ct);
            return sale is null ? NotFound() : Ok(sale);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/restore-line")]
    public async Task<ActionResult<SaleDto>> RestoreLine(Guid id, RestoreLineRequest request, CancellationToken ct)
    {
        try
        {
            var sale = await saleService.RestoreLineAsync(id, request, ct);
            return sale is null ? NotFound() : Ok(sale);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }
}
