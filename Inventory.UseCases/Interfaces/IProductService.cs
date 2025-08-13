using Inventory.UseCases.DTOs;

namespace Inventory.UseCases.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchCriteria criteria);
}
