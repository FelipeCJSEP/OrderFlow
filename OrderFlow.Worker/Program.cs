using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Repositories;
using OrderFlow.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configurar o Banco de Dados e os Repositórios
var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? "Server=localhost,1433;Database=OrderFlowDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

// 2. Configurar o MassTransit com RabbitMQ e Política de Retry (Resiliência Sênior)
builder.Services.AddMassTransit(x =>
{
    // Registra o nosso consumidor
    x.AddConsumer<OrderSubmittedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration.GetConnectionString("RabbitMq")
            ?? "amqp://guest:guest@localhost:5672";

        cfg.Host(new Uri(rabbitMqHost));

        // Configura a fila 'order-submitted-queue'
        cfg.ReceiveEndpoint("order-submitted-queue", e =>
        {
            // Política de Resiliência: se houver falha, tenta 3 vezes com intervalo de 2 segundos
            // Se falhar nas 3 tentativas, o MassTransit joga automaticamente para a DLQ (Dead Letter Queue)
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));

            e.ConfigureConsumer<OrderSubmittedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();