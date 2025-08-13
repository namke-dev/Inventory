using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.DTOs;

namespace Inventory.API.Controllers;

/// <summary>
/// REST API controller for comprehensive product management in the inventory system.
/// Provides full CRUD operations and advanced search capabilities for retail product management.
/// </summary>
/// <remarks>
/// **Product Management API Endpoints:**
/// 
/// This controller implements a complete product management solution with:
/// - **CRUD Operations**: Create, Read, Update, Delete products
/// - **Advanced Search**: Multi-criteria search with intelligent ranking
/// - **Validation**: Comprehensive input validation with detailed error messages
/// - **Performance**: Optimized queries with database indexing
/// - **Monitoring**: Structured logging for operations tracking
/// 
/// **API Endpoints Overview:**
/// 
/// | Method | Endpoint | Description |
/// |--------|----------|-------------|
/// | POST | `/api/products` | Create a new product |
/// | GET | `/api/products/{id}` | Get product by ID |
/// | PUT | `/api/products/{id}` | Update existing product |
/// | DELETE | `/api/products/{id}` | Delete product |
/// | GET | `/api/products` | Search products with filters |
/// 
/// **Common Response Codes:**
/// - `200 OK` - Request processed successfully
/// - `201 Created` - Resource created successfully
/// - `204 No Content` - Delete operation completed
/// - `400 Bad Request` - Invalid input or validation errors
/// - `404 Not Found` - Requested resource not found
/// - `500 Internal Server Error` - Unexpected server error
/// 
/// **Error Response Format:**
/// ```json
/// {
///   "type": "validation",
///   "title": "Validation Error",
///   "errors": [
///     "Product name is required",
///     "Price must be greater than 0"
///   ]
/// }
/// ```
/// 
/// **Performance Features:**
/// - Database indexing for fast searches
/// - Intelligent search result ranking
/// - Efficient pagination for large datasets
/// - Query optimization with minimal data transfer
/// 
/// **Security Features:**
/// - Input validation and sanitization
/// - Structured error handling without information leakage
/// - Comprehensive audit logging
/// 
/// **Best Practices Applied:**
/// - RESTful API design principles
/// - Comprehensive documentation with examples
/// - Consistent error handling patterns
/// - Performance monitoring and logging
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductsController with required dependencies.
    /// </summary>
    /// <param name="productService">Service layer for product business logic and data operations</param>
    /// <param name="logger">Structured logger for monitoring, debugging, and audit trails</param>
    /// <exception cref="ArgumentNullException">Thrown when productService or logger is null</exception>
    /// <remarks>
    /// This constructor implements dependency injection pattern for:
    /// - **Product Service**: Handles business logic, validation, and data access coordination
    /// - **Logger**: Provides structured logging for operations tracking and debugging
    /// 
    /// Dependencies are validated at construction time to ensure proper initialization.
    /// </remarks>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new product in the inventory system.
    /// </summary>
    /// <param name="createProductDto">Product data including name, description, category, price, and stock quantity</param>
    /// <returns>Created product with assigned ID and timestamps</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Creates a new product with comprehensive validation and business rule enforcement.
    /// 
    /// **Validation Rules:**
    /// - **Name**: Required, maximum 200 characters, should be descriptive
    /// - **Price**: Must be positive (greater than 0)
    /// - **Stock Quantity**: Must be non-negative (0 or greater)
    /// - **Category**: Required, maximum 100 characters
    /// - **Description**: Optional, maximum 500 characters
    /// 
    /// **Business Rules:**
    /// - Product ID is automatically generated as GUID
    /// - CreatedAt and UpdatedAt timestamps are set to current UTC time
    /// - Price is stored with decimal precision for accurate monetary calculations
    /// - Stock quantity of 0 indicates out-of-stock status
    /// 
    /// **Example Request:**
    /// ```
    /// POST /api/products
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "Gaming Laptop",
    ///   "description": "High-performance gaming laptop with RTX 4080",
    ///   "category": "Electronics",
    ///   "price": 1999.99,
    ///   "stockQuantity": 25
    /// }
    /// ```
    /// 
    /// **Example Success Response (201 Created):**
    /// ```json
    /// {
    ///   "id": "12345678-1234-1234-1234-123456789012",
    ///   "name": "Gaming Laptop",
    ///   "description": "High-performance gaming laptop with RTX 4080",
    ///   "category": "Electronics",
    ///   "price": 1999.99,
    ///   "stockQuantity": 25,
    ///   "createdAt": "2024-08-14T10:30:00Z",
    ///   "updatedAt": "2024-08-14T10:30:00Z"
    /// }
    /// ```
    /// 
    /// **Error Response Example (400 Bad Request):**
    /// ```json
    /// [
    ///   "Product name is required",
    ///   "Price must be greater than 0"
    /// ]
    /// ```
    /// </remarks>
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

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the product to retrieve</param>
    /// <returns>Product details if found</returns>
    /// <response code="200">Product found and returned successfully</response>
    /// <response code="404">Product not found with the specified ID</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Retrieves a single product from the inventory system using its unique ID.
    /// 
    /// Example request:
    /// ```
    /// GET /api/products/12345678-1234-1234-1234-123456789012
    /// ```
    /// 
    /// Example response:
    /// ```json
    /// {
    ///   "id": "12345678-1234-1234-1234-123456789012",
    ///   "name": "Gaming Laptop",
    ///   "description": "High-performance gaming laptop",
    ///   "category": "Electronics",
    ///   "price": 1999.99,
    ///   "stockQuantity": 25,
    ///   "createdAt": "2024-08-14T10:30:00Z",
    ///   "updatedAt": "2024-08-14T10:30:00Z"
    /// }
    /// ```
    /// </remarks>
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

    /// <summary>
    /// Updates an existing product in the inventory system.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="updateProductDto">Updated product information</param>
    /// <returns>Updated product details</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="404">Product not found with the specified ID</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Updates an existing product with new information. All fields in the request body will replace the current values.
    /// 
    /// Validation Rules:
    /// - Name: Required, maximum 200 characters
    /// - Price: Must be positive (greater than 0)
    /// - Stock quantity: Must be non-negative (0 or greater)
    /// - Category: Required, maximum 100 characters
    /// - Description: Optional, maximum 500 characters
    /// 
    /// Example request:
    /// ```
    /// PUT /api/products/12345678-1234-1234-1234-123456789012
    /// {
    ///   "name": "Gaming Laptop Pro",
    ///   "description": "Ultimate gaming laptop with RTX 4090",
    ///   "category": "Electronics",
    ///   "price": 2499.99,
    ///   "stockQuantity": 15
    /// }
    /// ```
    /// 
    /// Note: The UpdatedAt timestamp is automatically set to the current time.
    /// </remarks>
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

    /// <summary>
    /// Deletes a product from the inventory system.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>No content if deletion was successful</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found with the specified ID</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Permanently removes a product from the inventory system. This action cannot be undone.
    /// 
    /// Example request:
    /// ```
    /// DELETE /api/products/12345678-1234-1234-1234-123456789012
    /// ```
    /// 
    /// Success response: HTTP 204 No Content (empty response body)
    /// 
    /// **Warning:** This operation permanently deletes the product and all associated data.
    /// Consider implementing soft delete or archiving for production systems if product history needs to be preserved.
    /// 
    /// Business Rules:
    /// - Product must exist to be deleted
    /// - Deletion is immediate and permanent
    /// - No cascading delete operations (no related entities in current model)
    /// </remarks>
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
    /// <returns>Paginated list of products matching the search criteria</returns>
    /// <response code="200">Search completed successfully with results</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Performs advanced product search with multiple filtering options, intelligent ranking, and pagination.
    /// 
    /// **Search Features:**
    /// - **Keyword Search**: Case-insensitive search across name, description, and category
    /// - **Price Range**: Filter by minimum and/or maximum price
    /// - **Stock Status**: Filter by in-stock, out-of-stock, or all products
    /// - **Pagination**: Control page size and navigate through large result sets
    /// - **Sorting**: Multiple sort options with ascending/descending order
    /// 
    /// **Performance Optimizations:**
    /// - Database indexes on searchable fields for fast queries
    /// - Intelligent relevance ranking (exact matches first)
    /// - Efficient pagination with Skip/Take
    /// - Query optimization with AsNoTracking
    /// 
    /// **Query Parameters:**
    /// 
    /// | Parameter | Type | Description | Example |
    /// |-----------|------|-------------|---------|
    /// | `keyword` | string | Search term for name/description/category | `laptop` |
    /// | `minPrice` | decimal | Minimum price filter | `100.00` |
    /// | `maxPrice` | decimal | Maximum price filter | `2000.00` |
    /// | `inStock` | boolean | Filter by stock availability | `true` |
    /// | `page` | integer | Page number (1-based) | `1` |
    /// | `pageSize` | integer | Items per page (max 100) | `20` |
    /// | `sort` | string | Sort field and direction | `price_desc` |
    /// 
    /// **Sort Options:**
    /// - `name`, `name_desc` - Sort by product name
    /// - `price`, `price_desc` - Sort by price
    /// - `created`, `created_desc` - Sort by creation date
    /// - `stock`, `stock_desc` - Sort by stock quantity
    /// 
    /// **Example Requests:**
    /// ```
    /// GET /api/products                                    // Get all products (page 1)
    /// GET /api/products?keyword=laptop                     // Search for "laptop"
    /// GET /api/products?keyword=laptop&amp;minPrice=500        // Laptops over $500
    /// GET /api/products?category=Electronics&amp;inStock=true  // In-stock electronics
    /// GET /api/products?page=2&amp;pageSize=50&amp;sort=price_desc // Page 2, 50 items, by price descending
    /// ```
    /// 
    /// **Response Format:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "12345678-1234-1234-1234-123456789012",
    ///       "name": "Gaming Laptop",
    ///       "price": 1999.99,
    ///       "category": "Electronics",
    ///       "stockQuantity": 25
    ///     }
    ///   ],
    ///   "totalCount": 150,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "totalPages": 8
    /// }
    /// ```
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
