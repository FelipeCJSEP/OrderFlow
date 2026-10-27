using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Events
{
    public record OrderCreatedDomainEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
    {
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
