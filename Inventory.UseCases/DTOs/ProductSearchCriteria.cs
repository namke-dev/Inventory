namespace Inventory.UseCases.DTOs;

public class ProductSearchCriteria
{
    public string? Keyword { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "name";
}
