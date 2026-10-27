using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Entities
{
    public class OrderItem : Entity
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = "";
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public decimal Subtotal => UnitPrice * Quantity;

        private OrderItem() { } // construtor privado vazio para o EF Core

        public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity) : base(Guid.NewGuid())
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
            ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);

            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
    }
}
