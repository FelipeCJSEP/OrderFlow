using OrderFlow.Domain.Common;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;

namespace OrderFlow.Domain.Entities
{
    public class Order : Entity
    {
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        private readonly List<OrderItem> _items = [];
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public decimal TotalAmount => _items.Sum(i => i.Subtotal);

        private Order() { } // construtor privado vazio para o EF Core

        private Order(Guid id, Guid customerId) : base(id)
        {
            CustomerId = customerId;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Order Create(Guid customerId, List<OrderItem> items)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("O identificador do cliente é obrigatório.", nameof(customerId));

            ArgumentNullException.ThrowIfNull(items);

            if (items.Count == 0)
                throw new ArgumentException("O pedido deve conter ao menos um item.", nameof(items));

            Order order = new(Guid.NewGuid(), customerId);
            order._items.AddRange(items);
            order.AddEvent(new OrderCreatedDomainEvent(order.Id, customerId, order.TotalAmount));

            return order;
        }

        public void MarkAsProcessing() 
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Apenas pedidos pendentes podem entrar em processamento.");

            Status = OrderStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsApproved()
        {
            if (Status != OrderStatus.Processing)
                throw new InvalidOperationException("Apenas pedidos em processamento podem ser aprovados.");

            Status = OrderStatus.Approved;
            UpdatedAt = DateTime.UtcNow;
            AddEvent(new OrderApprovedDomainEvent(Id));
        }

        public void MarkAsFailed(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            if (Status == OrderStatus.Approved)
                throw new InvalidOperationException("Não é possível rejeitar um pedido já aprovado.");

            Status = OrderStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
            AddEvent(new OrderFailedDomainEvent(Id, reason));
        }
    }
}
