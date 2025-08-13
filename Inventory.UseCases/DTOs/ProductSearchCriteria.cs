namespace Inventory.UseCases.DTOs;

/// <summary>
/// Search criteria for filtering and sorting products.
/// All parameters are optional for flexible search capabilities.
/// </summary>
/// <remarks>
/// Search Features:
/// - Keyword search across name, description, and category
/// - Price range filtering with min/max bounds
/// - Stock availability filtering
/// - Pagination with configurable page size
/// - Multi-field sorting with direction control
/// 
/// Performance: Optimized with database indexes for fast search operations.
/// </remarks>
public class ProductSearchCriteria
{
    /// <summary>
    /// Search keyword to match against product name, description, and category.
    /// Case-insensitive partial matching with intelligent relevance ranking.
    /// </summary>
    /// <example>laptop</example>
    public string? Keyword { get; set; }
    
    /// <summary>
    /// Minimum price filter (inclusive).
    /// Products with price greater than or equal to this value will be included.
    /// </summary>
    /// <example>100.00</example>
    public decimal? MinPrice { get; set; }
    
    /// <summary>
    /// Maximum price filter (inclusive).
    /// Products with price less than or equal to this value will be included.
    /// </summary>
    /// <example>2000.00</example>
    public decimal? MaxPrice { get; set; }
    
    /// <summary>
    /// Filter by stock availability.
    /// - true: Only products with stock quantity > 0
    /// - false: Only products with stock quantity = 0 (out of stock)
    /// - null: All products regardless of stock status
    /// </summary>
    /// <example>true</example>
    public bool? InStock { get; set; }
    
    /// <summary>
    /// Page number for pagination (1-based).
    /// Defaults to 1 if not specified.
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page for pagination.
    /// Defaults to 20 if not specified. Maximum recommended: 100.
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Sort field and direction.
    /// Supported values: name, name_desc, price, price_desc, created, created_desc, stock, stock_desc.
    /// Defaults to 'name' if not specified.
    /// </summary>
    /// <example>price_desc</example>
    public string Sort { get; set; } = "name";
}
