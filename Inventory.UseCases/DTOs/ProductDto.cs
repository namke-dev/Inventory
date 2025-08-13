namespace Inventory.UseCases.DTOs;

/// <summary>
/// Product information returned by the API.
/// Contains all product details including system-generated fields.
/// </summary>
/// <remarks>
/// This DTO represents the complete product information as returned by API endpoints.
/// All fields are read-only from the client perspective.
/// </remarks>
public class ProductDto
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    /// <example>12345678-1234-1234-1234-123456789012</example>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Display name of the product (max 200 characters).
    /// </summary>
    /// <example>Gaming Laptop</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed product description (max 500 characters).
    /// </summary>
    /// <example>High-performance gaming laptop with RTX 4080 graphics card</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category for organization and filtering (max 100 characters).
    /// </summary>
    /// <example>Electronics</example>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Current selling price of the product.
    /// </summary>
    /// <example>1999.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Current quantity available in inventory.
    /// Zero indicates out of stock.
    /// </summary>
    /// <example>25</example>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// UTC timestamp when the product was created.
    /// </summary>
    /// <example>2024-08-14T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// UTC timestamp when the product was last updated.
    /// </summary>
    /// <example>2024-08-14T15:45:00Z</example>
    public DateTime UpdatedAt { get; set; }
}
