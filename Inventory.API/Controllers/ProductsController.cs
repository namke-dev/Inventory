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
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Searches and filters products with advanced criteria and pagination.
    /// </summary>
    /// <param name="criteria">Search criteria including keyword, price range, stock status, pagination, and sorting options</param>
    /// <remarks>
    /// 
    /// **Query Parameters:**
    /// 
    /// | Parameter | Type | Description |
    /// |-----------|------|-------------|
    /// | `keyword` | string | Search term for name/description/category |
    /// | `minPrice` | decimal | Minimum price filter |
    /// | `maxPrice` | decimal | Maximum price filter |
    /// | `inStock` | boolean | Filter by stock availability |
    /// | `page` | integer | Page number (1-based) |
    /// | `pageSize` | integer | Items per page (max 100) |
    /// | `sort` | string | Sort field and direction |
    /// 
    /// **Sort Options:**
    /// - `name`, `name_desc` - Sort by product name
    /// - `price`, `price_desc` - Sort by price
    /// - `created`, `created_desc` - Sort by creation date
    /// - `stock`, `stock_desc` - Sort by stock quantity
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
