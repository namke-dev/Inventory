namespace Inventory.UseCases.DTOs;

/// <summary>
/// Request model for creating a new product.
/// Contains all required information to add a product to the inventory.
/// </summary>
/// <remarks>
/// Validation Rules:
/// - Name: Required, maximum 200 characters
/// - Price: Must be positive (greater than 0)
/// - StockQuantity: Must be non-negative (0 or greater)
/// - Category: Required, maximum 100 characters
/// - Description: Optional, maximum 500 characters
/// </remarks>
public class CreateProductDto
{
    /// <summary>
    /// Display name of the product.
    /// Must be unique and descriptive.
    /// </summary>
    /// <example>Gaming Laptop</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed product description.
    /// Optional field for additional product information.
    /// </summary>
    /// <example>High-performance gaming laptop with RTX 4080 graphics card</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category for organization and filtering.
    /// Used for grouping similar products.
    /// </summary>
    /// <example>Electronics</example>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Selling price of the product.
    /// Must be a positive decimal value.
    /// </summary>
    /// <example>1999.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Initial quantity to add to inventory.
    /// Must be zero or positive. Zero indicates out of stock.
    /// </summary>
    /// <example>25</example>
    public int StockQuantity { get; set; }
}
