using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.Entities;

/// <summary>
/// Core domain entity representing a product in the inventory management system.
/// Contains all essential product information and business rules for inventory tracking.
/// </summary>
/// <remarks>
/// Business Rules Enforced:
/// - Product names must be unique and descriptive (max 200 characters)
/// - Prices must be positive values (enforced at service level)
/// - Stock quantities must be non-negative (0 indicates out of stock)
/// - Categories are required for proper product organization
/// - Timestamps track creation and modification for audit purposes
/// </remarks>
public class Product
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
