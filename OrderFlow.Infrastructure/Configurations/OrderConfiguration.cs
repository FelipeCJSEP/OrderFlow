using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Configurations
{
    internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CustomerId).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Ignore(x => x.TotalAmount);
            builder.Ignore(x => x.DomainEvents);
            builder.HasMany(x => x.Items).WithOne().HasForeignKey("OrderId").IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
