using MediatR;

namespace OrderFlow.Application.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderItemDto(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity
    );

    public sealed record CreateOrderCommand(
        Guid CustomerId,
        IReadOnlyList<CreateOrderItemDto> Items
    ) : IRequest<Guid>;
}
