using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Repositories;

namespace OrderFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database") ?? "Server=localhost,1433;Database=OrderFlowDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;";

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddMassTransit(busConfig =>
            {
                // Configura o Outbox para gravar eventos no mesmo DbContext/transação
                busConfig.AddEntityFrameworkOutbox<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer();
                    options.UseBusOutbox();
                });

                // Conecta no RabbitMQ
                busConfig.UsingRabbitMq((context, config) =>
                {
                    var rabbitMqConnection = configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672";

                    config.Host(new Uri(rabbitMqConnection));
                    config.ConfigureEndpoints(context);
                });

            });

            return services;
        }
    }
}
