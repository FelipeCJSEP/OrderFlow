using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.Queries.GetOrderById;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController(ISender sender) :  ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var orderId = await sender.Send(command, cancellationToken);
            
            return CreatedAtAction(nameof(GetById), new { id = orderId }, new { id = orderId });
        }
    }
}
