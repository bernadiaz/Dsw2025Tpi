using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagmentService _service;
    public OrdersController(IOrdersManagmentService _ordersManagmentService)
    {
        _service = _ordersManagmentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest _request)
    {
        try
        {
            var _order = await _service.AddOrder(_request);
            return Created("api/order", _order);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (InvalidOperationException io)
        {
            return BadRequest(io.Message);
        }
        catch (EntityNotFoundException enfe)
        {
            return BadRequest(enfe.Message);
        }
        catch (DuplicatedEntityException dee)
        {
            return BadRequest(dee.Message);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
