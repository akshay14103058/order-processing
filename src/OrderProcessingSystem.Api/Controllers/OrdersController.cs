using Microsoft.AspNetCore.Mvc;
using OrderProcessingSystem.Application.DTOs;
using OrderProcessingSystem.Application.Interfaces;
using OrderProcessingSystem.Domain.Entities;
using FluentValidation;

namespace OrderProcessingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _validator;

    public OrdersController(IOrderService orderService, IValidator<CreateOrderDto> validator)
    {
        _orderService = orderService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var validationResult = await _validator.ValidateAsync(createOrderDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var order = await _orderService.CreateOrderAsync(createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] string? status)
    {
        OrderStatus? filter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            filter = parsedStatus;
        }

        var orders = await _orderService.GetAllOrdersAsync(filter);
        return Ok(orders);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        await _orderService.CancelOrderAsync(id);
        return NoContent();
    }
}
