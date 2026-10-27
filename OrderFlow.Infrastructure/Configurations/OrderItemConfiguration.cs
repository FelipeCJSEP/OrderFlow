using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Configurations
{
    internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.ProductName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Quantity).IsRequired();
            builder.Ignore(x => x.Subtotal);
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
