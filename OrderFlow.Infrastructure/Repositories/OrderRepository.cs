using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Repositories
{
    public sealed class OrderRepository(ApplicationDbContext context) : IOrderRepository
    {
        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
            => await context.Orders.AddAsync(order, cancellationToken);

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        public void Update(Order order)
            => context.Orders.Update(order);
    }
}
