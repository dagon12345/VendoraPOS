using Microsoft.AspNetCore.Mvc;
using Vendora.Application.Products;

namespace Vendora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct) =>
        Ok(await productService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = await productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await productService.UpdateAsync(id, request, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ProductDto>> Activate(Guid id, CancellationToken ct)
    {
        var product = await productService.SetActiveAsync(id, true, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ProductDto>> Deactivate(Guid id, CancellationToken ct)
    {
        var product = await productService.SetActiveAsync(id, false, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("{id:guid}/audit-log")]
    public async Task<ActionResult<IReadOnlyList<ProductAuditLogDto>>> GetAuditLog(Guid id, CancellationToken ct)
    {
        var log = await productService.GetAuditLogAsync(id, ct);
        return log is null ? NotFound() : Ok(log);
    }
}
