using MediatR;

namespace Shop.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid SessionId,
    string TransactionHash,
    string WalletAddress) : IRequest<Guid?>;
