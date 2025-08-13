using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.DTOs;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            var result = await _productService.CreateProductAsync(createProductDto);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", result.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for product creation: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Update a product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            var result = await _productService.UpdateProductAsync(id, updateProductDto);
            if (result == null)
            {
                _logger.LogWarning("Product not found for update with ID: {ProductId}", id);
                return NotFound();
            }

            _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for product update: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                _logger.LogWarning("Product not found for deletion with ID: {ProductId}", id);
                return NotFound();
            }

            _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }

    /// <summary>
    /// Search products with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> SearchProducts([FromQuery] ProductSearchCriteria criteria)
    {
        try
        {
            var result = await _productService.SearchProductsAsync(criteria);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, "An error occurred while searching products");
        }
    }
}
