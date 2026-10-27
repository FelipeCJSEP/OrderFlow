using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Events
{
    public record OrderApprovedDomainEvent(Guid OrderId) : IDomainEvent
    {
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
