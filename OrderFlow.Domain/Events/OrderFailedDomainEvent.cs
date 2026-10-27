using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Events
{
    public record OrderFailedDomainEvent(Guid OrderId, string Reason) : IDomainEvent
    {
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
