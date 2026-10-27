using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;

namespace OrderFlow.UnitTests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void Create_WithValidData_ShouldCreateOrderAndRaiseDomainEvent()
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new(Guid.NewGuid(), "Mouse", 100.00m, 2),  // Subtotal: 200.00
                new(Guid.NewGuid(), "Teclado", 150.00m, 1) // Subtotal: 150.00
            };

            var order = Order.Create(customerId, items);

            order.Should().NotBeNull();
            order.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.Pending);
            order.TotalAmount.Should().Be(350.00m); // 200 + 150
            order.Items.Should().HaveCount(2);

            // Garante que o evento de criação foi registrado
            order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
        }

        [Fact]
        public void Create_WithEmptyCustomerId_ShouldThrowArgumentException()
        {
            var customerId = Guid.Empty;
            var items = new List<OrderItem>
            {
                new(Guid.NewGuid(), "Mouse", 100.00m, 2),  // Subtotal: 200.00
            };

            Action act = () => Order.Create(customerId, items);
            act.Should().Throw<ArgumentException>().WithMessage("*identificador do cliente é obrigatório*");
        }

        [Fact]
        public void Create_WithEmptyItems_ShouldThrowArgumentException()
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>();

            Action act = () => Order.Create(customerId, items);
            act.Should().Throw<ArgumentException>().WithMessage("*ao menos um item*");
        }

        [Fact]
        public void MarkAsProcessing_WhenPending_ShouldUpdateStatusAndTimestamp()
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new(Guid.NewGuid(), "Mouse", 100.00m, 2),  // Subtotal: 200.00
                new(Guid.NewGuid(), "Teclado", 150.00m, 1) // Subtotal: 150.00
            };

            var order = Order.Create(customerId, items);

            order.MarkAsProcessing();

            order.Status.Should().Be(OrderStatus.Processing);
            order.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkAsProcessing_WhenNotPending_ShouldThrowInvalidOperationException()
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new(Guid.NewGuid(), "Mouse", 100.00m, 2),  // Subtotal: 200.00
                new(Guid.NewGuid(), "Teclado", 150.00m, 1) // Subtotal: 150.00
            };

            var order = Order.Create(customerId, items);
            order.MarkAsProcessing();

            Action act = () => order.MarkAsProcessing();
            act.Should().Throw<InvalidOperationException>().WithMessage("*Apenas pedidos pendentes*");
        }

        [Fact]
        public void MarkAsApproved_WhenProcessing_ShouldUpdateStatusAndRaiseEvent()
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new(Guid.NewGuid(), "Mouse", 100.00m, 2),  // Subtotal: 200.00
                new(Guid.NewGuid(), "Teclado", 150.00m, 1) // Subtotal: 150.00
            };

            var order = Order.Create(customerId, items);
            order.MarkAsProcessing();
            order.MarkAsApproved();

            order.Status.Should().Be(OrderStatus.Approved);
            order.DomainEvents.Should().Contain(e => e is OrderApprovedDomainEvent);
        }
    }
}
