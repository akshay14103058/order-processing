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

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="createOrderDto">The order creation data.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">Returns the newly created order.</response>
    /// <response code="400">If validation fails.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Retrieves a specific order by unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">The order was found.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    /// <summary>
    /// Retrieves a list of all orders, optionally filtered by status.
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Processing, Shipped, Delivered, Cancelled).</param>
    /// <returns>A list of orders.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <remarks>
    /// Only orders in the 'Pending' state can be cancelled.
    /// </remarks>
    /// <param name="id">The unique identifier of the order to cancel.</param>
    /// <response code="204">Order was successfully cancelled.</response>
    /// <response code="400">Order could not be cancelled (e.g., already shipped).</response>
    /// <response code="404">Order was not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        await _orderService.CancelOrderAsync(id);
        return NoContent();
    }
}
