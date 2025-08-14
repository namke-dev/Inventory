using Inventory.Domain.Entities;

namespace Inventory.UseCases.Interfaces;

public interface IProductRepository
{
    Task<Product> CreateAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
    Task<(IList<Product> Items, int TotalCount)> SearchAsync(
        string? keyword,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        int page,
        int pageSize,
        string sort);
}


