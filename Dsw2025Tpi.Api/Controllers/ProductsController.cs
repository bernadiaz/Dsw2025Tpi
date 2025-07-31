using Dsw2025Ej15.Application.Dtos;
using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Ej15.Application.Services;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductsManagementService _service;
    public ProductsController(IProductsManagementService _productsManagementService)
    {
        _service = _productsManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var _products = await _service.GetProducts();
        if (_products == null || !_products.Any())
        {
            return NoContent();
        }
        return Ok(_products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var _product = await _service.GetProductById(id);
        if (_product == null)
        {
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
            return BadRequest(ae.Message);
        }
        catch (DuplicatedEntityException dee)
        {
            return Conflict(dee.Message);

        }
        catch (Exception ex)
        {
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
                return NotFound($"No se encontró el producto con Id {id}");
            }
            return Ok(_product);
        }
        catch (EntityNotFoundException enfe)
        {
            return NotFound(enfe.Message);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (Exception ex)
        {
            return Problem("Se produjo un error al actualizar el producto");
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
            return NotFound(enfe.Message);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (DuplicatedEntityException de)
        {
            return BadRequest(de.Message);
        }
        catch (Exception ex)
        {
            return Problem("Se produjo un error al actualizar el producto");
        }
    }
}
