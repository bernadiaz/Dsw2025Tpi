using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Dsw2025TpiContext _context;

        public ProductsController(Dsw2025TpiContext context)
        {
            _context = context;
        }

        //POST
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.Name) || product.CurrentUnitPrice <= 0 || product.StockQuantity < 0)
                return BadRequest("Datos inválidos: Sku, Name obligatorios, precio debe ser > 0 y stock >= 0");

            if (await _context.Products.AnyAsync(p => p.Sku == product.Sku))
                return BadRequest("Ya existe un producto con ese SKU.");

            product.IsActive = true;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        //GET
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
            if (products.Count == 0) return NoContent();
            return Ok(products);
        }

        //GET (por id)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
                return NotFound();
            return Ok(product);
        }

        //PUT (por id)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product updated)
        {
            if (id != updated.Id) return BadRequest("El id no coincide.");
            if (string.IsNullOrWhiteSpace(updated.Sku) || string.IsNullOrWhiteSpace(updated.Name) || updated.CurrentUnitPrice <= 0 || updated.StockQuantity < 0)
                return BadRequest("Datos inválidos: Sku, Name obligatorios, precio debe ser > 0 y stock >= 0");

            var existing = await _context.Products.FindAsync(id);
            if (existing == null || !existing.IsActive) return NotFound();

            if (await _context.Products.AnyAsync(p => p.Sku == updated.Sku && p.Id != id))
                return BadRequest("Ya existe otro producto con ese SKU.");

            //actualizo los campos
            existing.Sku = updated.Sku;
            existing.InternalCode = updated.InternalCode;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.CurrentUnitPrice = updated.CurrentUnitPrice;
            existing.StockQuantity = updated.StockQuantity;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        //PATCH (por id)
        [HttpPatch("{id}")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive) return NotFound();

            product.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}