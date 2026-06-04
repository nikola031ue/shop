using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Carts.Commands.AddCartItem;
using Shop.Application.Carts.Commands.ClearCart;
using Shop.Application.Carts.Commands.RemoveCartItem;
using Shop.Application.Carts.Commands.UpdateCartItem;
using Shop.Application.Carts.Queries.GetCart;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        SetSessionHeader(sessionId);
        var cart = await mediator.Send(new GetCartQuery(sessionId), cancellationToken);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        SetSessionHeader(sessionId);
        var cartItemId = await mediator.Send(new AddCartItemCommand(sessionId, request.ProductId, request.Quantity), cancellationToken);
        if (cartItemId is null)
            return NotFound($"Proizvod {request.ProductId} nije pronađen.");
        return CreatedAtAction(nameof(GetCart), new { }, new { cartItemId });
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        var updated = await mediator.Send(new UpdateCartItemCommand(sessionId, id, request.Quantity), cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        var removed = await mediator.Send(new RemoveCartItemCommand(sessionId, id), cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        await mediator.Send(new ClearCartCommand(sessionId), cancellationToken);
        return NoContent();
    }

    private Guid GetSessionId()
    {
        var header = Request.Headers["X-Session-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.NewGuid();
    }

    private void SetSessionHeader(Guid sessionId) =>
        Response.Headers["X-Session-Id"] = sessionId.ToString();
}

public record AddCartItemRequest(Guid ProductId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
