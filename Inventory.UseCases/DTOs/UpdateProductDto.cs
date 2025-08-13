namespace Inventory.UseCases.DTOs;

/// <summary>
/// Request model for updating an existing product.
/// Contains all modifiable product fields.
/// </summary>
/// <remarks>
/// Validation Rules:
/// - Name: Required, maximum 200 characters
/// - Price: Must be positive (greater than 0)
/// - StockQuantity: Must be non-negative (0 or greater)
/// - Category: Required, maximum 100 characters
/// - Description: Optional, maximum 500 characters
/// 
/// Note: The product ID is provided in the URL path, not in this model.
/// CreatedAt and UpdatedAt timestamps are managed automatically.
/// </remarks>
public class UpdateProductDto
{
    /// <summary>
    /// Updated display name of the product.
    /// Must be unique and descriptive.
    /// </summary>
    /// <example>Gaming Laptop Pro</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated detailed product description.
    /// Optional field for additional product information.
    /// </summary>
    /// <example>Ultimate gaming laptop with RTX 4090 graphics card</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated product category for organization and filtering.
    /// Used for grouping similar products.
    /// </summary>
    /// <example>Electronics</example>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated selling price of the product.
    /// Must be a positive decimal value.
    /// </summary>
    /// <example>2499.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Updated quantity available in inventory.
    /// Must be zero or positive. Zero indicates out of stock.
    /// </summary>
    /// <example>15</example>
    public int StockQuantity { get; set; }
}
