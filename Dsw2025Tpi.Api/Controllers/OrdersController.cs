using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagmentService _service;

    public OrdersController(IOrdersManagmentService service)
    {
        _service = service;
    }

    //POST - creo una nueva orden
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        try
        {
            var result = await _service.AddOrder(request);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    //GET - obtengo todas las ordenes con filtro que son opcionales
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var orders = await _service.GetOrders(status, customerId, pageNumber, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    //GET - obtengo las ordenes por el id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var order = await _service.GetOrderById(id);
            return order == null ? NotFound() : Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    //PUT - cambio de estado la orden
    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderModel.StatusUpdateRequest request)
    {
        try
        {
            var updated = await _service.UpdateOrderStatus(id, request.NewStatus);
            return Ok(updated);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

