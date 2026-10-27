using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Commands.CreateOrder;

namespace OrderFlow.UnitTests.Application
{
    public class CreateOrderCommandValidatorTests
    {
        private readonly CreateOrderCommandValidator _validator = new();

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var command = new CreateOrderCommand(
                Guid.NewGuid(),
                new List<CreateOrderItemDto>()
                {
                    new(Guid.NewGuid(), "Teclado Mecânico", 250.00m, 1)
                }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyCustomerId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateOrderCommand(
                Guid.Empty, // Inválido!
                new List<CreateOrderItemDto>
                {
                new(Guid.NewGuid(), "Mouse", 100.00m, 1)
                }
            );
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }
        [Fact]
        public void Validate_WithEmptyItems_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateOrderCommand(
                Guid.NewGuid(),
                new List<CreateOrderItemDto>() // Lista vazia!
            );
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items);
        }
        [Fact]
        public void Validate_WithNegativeQuantity_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateOrderCommand(
                Guid.NewGuid(),
                new List<CreateOrderItemDto>
                {
                new(Guid.NewGuid(), "Mouse", 100.00m, -5) // Quantidade negativa!
                }
            );
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }
    }
}
