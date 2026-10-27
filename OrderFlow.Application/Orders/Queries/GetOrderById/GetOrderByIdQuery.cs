using MediatR;

namespace OrderFlow.Application.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponse?>;
}
