using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Orders.Commands.CreateOrder;
using Shop.Application.Orders.Commands.UpdateOrderStatus;
using Shop.Application.Orders.Queries.GetOrderById;
using Shop.Application.Orders.Queries.GetOrders;
using Shop.Domain.Enums;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    // TODO: [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOrdersQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        if (sessionId == Guid.Empty)
            return BadRequest("X-Session-Id header je obavezan.");

        var orderId = await mediator.Send(
            new CreateOrderCommand(sessionId, request.TransactionHash, request.WalletAddress),
            cancellationToken);

        if (orderId is null)
            return BadRequest("Korpa je prazna ili ne postoji.");

        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });
    }

    // TODO: [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
            return BadRequest($"Nepoznat status: {request.Status}. Dostupni: {string.Join(", ", Enum.GetNames<OrderStatus>())}");

        var updated = await mediator.Send(new UpdateOrderStatusCommand(id, status), cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    private Guid GetSessionId()
    {
        var header = Request.Headers["X-Session-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.Empty;
    }
}

public record CreateOrderRequest(string TransactionHash, string WalletAddress);
public record UpdateOrderStatusRequest(string Status);
