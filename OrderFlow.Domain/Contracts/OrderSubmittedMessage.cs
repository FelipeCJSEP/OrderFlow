namespace OrderFlow.Domain.Contracts
{
    public sealed record OrderSubmittedMessage(Guid OrderId, Guid CustomerId, decimal TotalAmount);
}
