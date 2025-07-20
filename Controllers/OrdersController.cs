using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models.DTOs;
using OrdersApi.Services;
using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Controllers;

/// <summary>
/// Orders management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create new order
    /// </summary>
    /// <param name="request">Create order request</param>
    /// <returns>Created order information</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            _logger.LogInformation("Received create order request, customer name: {CustomerName}", request.CustomerName);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create order request validation failed, customer name: {CustomerName}", request.CustomerName);
                return BadRequest(ModelState);
            }

            // Call service layer to create order
            var response = await _orderService.CreateOrderAsync(request);

            _logger.LogInformation("Order created successfully, order ID: {OrderId}", response.OrderId);
            return CreatedAtAction(
                nameof(CreateOrder), 
                new { id = response.OrderId }, 
                response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Order creation failed: data validation error");
            return BadRequest(new ProblemDetails
            {
                Title = "Data validation failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error",
                Detail = "An error occurred while creating order, please try again later",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
} 