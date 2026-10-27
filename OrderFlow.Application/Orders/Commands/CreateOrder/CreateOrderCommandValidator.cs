using FluentValidation;

namespace OrderFlow.Application.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("O identificador do cliente é obrigatório.");
            RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve conter ao menos um item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("O produto é obrigatório.");

                item.RuleFor(i => i.ProductName)
                    .NotEmpty().WithMessage("O nome do produto é obrigatório.")
                    .MaximumLength(100).WithMessage("O nome do produto deve ter no máximo 100 caracteres.");

                item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
            });
        }
    }
}
