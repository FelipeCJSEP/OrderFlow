using MediatR;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Orders.Queries.GetOrderById
{
    internal sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
    {
        public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order is null)
            {
                return null;
            }

            var itemsResponse = order.Items.Select(item => new OrderItemResponse(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.Subtotal
            )).ToList();

            var orderResponse = new OrderResponse(
                order.Id,
                order.CustomerId,
                order.Status.ToString(),
                order.TotalAmount,
                order.CreatedAt,
                order.UpdatedAt,
                itemsResponse
            );

            return orderResponse;
        }
    }
}
