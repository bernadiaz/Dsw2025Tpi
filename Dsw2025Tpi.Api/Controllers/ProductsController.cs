using Dsw2025Ej15.Application.Dtos;
using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Ej15.Application.Services;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductsManagementService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductsManagementService _productsManagementService, ILogger<ProductsController> logger)
    {
        _service = _productsManagementService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var _products = await _service.GetProducts();
        if (_products == null || !_products.Any())
        {
            _logger.LogInformation("No se encontraron productos");
            return NoContent();
        }
        return Ok(_products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var _product = await _service.GetProductById(id);
        if (_product == null)
        {
            _logger.LogInformation("No se encontró el producto con Id: {Id}", id);
            return NotFound($"No se encontró el producto con Id {id}");
        }
        return Ok(_product);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.ProductRequest _request)
    {
        try
        {
            var _product = await _service.AddProduct(_request);
            return Created("/product", _product);
        }
        catch (ArgumentException ae)
        {
            _logger.LogWarning("Error al agregar el producto: {Message}", ae.Message);
            return BadRequest(ae.Message);
        }
        catch (DuplicatedEntityException dee)
        {
            _logger.LogWarning("Error al agregar el producto: {Message}", dee.Message);
            return Conflict(dee.Message);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el producto");
            return Problem("Se produjo un error al guardar el producto");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> DisableProduct(Guid id)
    {
        try
        {
            var _product = await _service.DeleteProduct(id);
            if (_product == null)
            {
                _logger.LogInformation("No se encontró el producto con Id: {Id}", id);
                return NotFound($"No se encontró el producto con Id {id}");
            }
            return Ok(_product);
        }
        catch (EntityNotFoundException enfe)
        {
            _logger.LogError(enfe, "Error al eliminar el producto");
            return NotFound(enfe.Message);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el producto");
            return Problem("Se produjo un error al eliminar el producto");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductById(Guid id, [FromBody] ProductModel.ProductRequest _request)
    {
        try
        {
            var _product = await _service.UpdateProduct(id, _request);
            return Ok(_product);
        }
        catch (EntityNotFoundException enfe)
        {
            _logger.LogError(enfe, "Error al actualizar el producto: {message}", enfe.Message);
            return NotFound(enfe.Message);
        }
        catch (ArgumentException ae)
        {
            _logger.LogError(ae, "Error al actualizar el producto: {message}", ae.Message);
            return BadRequest(ae.Message);
        }
        catch (DuplicatedEntityException de)
        {
            _logger.LogError(de, "Error al actualizar el producto: {message}", de.Message);
            return BadRequest(de.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el producto");
            return Problem("Se produjo un error al actualizar el producto");
        }
    }
}
