using MassTransit;
using MediatR;
using OrderFlow.Domain.Contracts;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Orders.Commands.CreateOrder
{
    internal sealed class CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderItems = request.Items.Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity)).ToList();
            var order = Order.Create(request.CustomerId, orderItems);

            await orderRepository.AddAsync(order, cancellationToken);

            // Publica mensagem no Outbox
            await publishEndpoint.Publish(new OrderSubmittedMessage(order.Id, order.CustomerId, order.TotalAmount), cancellationToken);

            // O SaveChanges grava o pedido E a mensagem do outbox na mesma transação do SQL Server
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
