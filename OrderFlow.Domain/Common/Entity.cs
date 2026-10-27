namespace OrderFlow.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected Entity(Guid id) => Id = id;

        protected Entity() {}

        public void AddEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearEvent() => _domainEvents.Clear();
    }
}
