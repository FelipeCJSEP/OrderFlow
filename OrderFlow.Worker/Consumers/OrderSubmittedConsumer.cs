using MassTransit;
using OrderFlow.Domain.Contracts;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Worker.Consumers
{
    public sealed class OrderSubmittedConsumer(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<OrderSubmittedConsumer> logger) : IConsumer<OrderSubmittedMessage>
    {
        public async Task Consume(ConsumeContext<OrderSubmittedMessage> context)
        {
            logger.LogInformation(
                "Mensagem recebida para o pedido {OrderId}. Valor total: {TotalAmount:C}",
                context.Message.OrderId,
                context.Message.TotalAmount
            );

            var order = await orderRepository.GetByIdAsync(context.Message.OrderId, context.CancellationToken);

            if (order == null)
            {
                logger.LogWarning("Pedido {OrderId} não encontrado para processamento.", context.Message.OrderId);
                return;
            }

            // Mudar status para Processing e salvar
            order.MarkAsProcessing();
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Pedido {OrderId} em processamento de pagamento...", order.Id);

            // Simular chamada ao Gateway de Pagamento (delay de 2 segundos):
            await Task.Delay(2000, context.CancellationToken);

            // Mudar status para Approved e salvar:
            order.MarkAsApproved();
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Pedido {OrderId} aprovado e concluído com sucesso!", order.Id);
        }
    }
}
