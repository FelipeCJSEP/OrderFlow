using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.Queries.GetOrderById;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.IntegrationTests
{
    public class OrdersApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateOrder_WithValidPayload_ShouldReturn201CreatedAndPersistInDatabase()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var command = new CreateOrderCommand(
                customerId,
                new List<CreateOrderItemDto>
                {
                    new(Guid.NewGuid(), "Monitor Dell 27", 1800.00m, 1),
                    new(Guid.NewGuid(), "Mousepad Gamer", 80.00m, 2)
                }
            );

            // Act: Envia a requisição HTTP real para a API
            var response = await _client.PostAsJsonAsync("/api/Orders", command);

            // Assert HTTP
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            result.Should().NotBeNull();
            result!.Id.Should().NotBeEmpty();

            // Assert Banco de Dados: Valida se o registro foi salvo no SQL Server do Testcontainers
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var orderInDb = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == result.Id);

            orderInDb.Should().NotBeNull();
            orderInDb!.CustomerId.Should().Be(customerId);
            orderInDb.Status.Should().Be(OrderStatus.Pending);
            orderInDb.TotalAmount.Should().Be(1960.00m); // 1800 + (80 * 2)
            orderInDb.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateOrder_WithInvalidPayload_ShouldReturn400BadRequest()
        {
            // Arrange: comando com cliente vazio e sem itens
            var command = new CreateOrderCommand(Guid.Empty,new List<CreateOrderItemDto>());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Orders", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ShouldReturn200OkWithDetails()
        {
            // Arrange: Primeiro cria um pedido válido via API
            var command = new CreateOrderCommand(
                Guid.NewGuid(),
                new List<CreateOrderItemDto>
                {
                    new(Guid.NewGuid(), "Cadeira Gamer", 1200.00m, 1)
                }
            );

            var createResponse = await _client.PostAsJsonAsync("/api/Orders", command);
            var createdResult = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();

            // Act: Busca o pedido criado pelo endpoint GET
            var getResponse = await _client.GetAsync($"/api/Orders/{createdResult!.Id}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var orderDetails = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
            orderDetails.Should().NotBeNull();
            orderDetails!.Id.Should().Be(createdResult.Id);
            orderDetails.Status.Should().Be("Pending");
            orderDetails.TotalAmount.Should().Be(1200.00m);
        }

        // DTO auxiliar para ler a resposta do POST
        private sealed record CreateOrderResponse(Guid Id);
    }
}
