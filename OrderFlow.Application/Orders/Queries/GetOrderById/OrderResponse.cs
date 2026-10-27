namespace OrderFlow.Application.Orders.Queries.GetOrderById
{
    public sealed record OrderItemResponse(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal Subtotal
    );

    public sealed record OrderResponse(
        Guid Id,
        Guid CustomerId,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        IReadOnlyList<OrderItemResponse> Items
    );
}
