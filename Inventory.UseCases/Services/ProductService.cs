using FluentValidation;
using Inventory.Domain.Entities;
using Inventory.UseCases.DTOs;
using Inventory.UseCases.Interfaces;

namespace Inventory.UseCases.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductService(
        IProductRepository productRepository,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var validationResult = await _createValidator.ValidateAsync(createProductDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Category = createProductDto.Category,
            Price = createProductDto.Price,
            StockQuantity = createProductDto.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProduct = await _productRepository.CreateAsync(product);
        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateProductDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return null;
        }

        existingProduct.Name = updateProductDto.Name;
        existingProduct.Description = updateProductDto.Description;
        existingProduct.Category = updateProductDto.Category;
        existingProduct.Price = updateProductDto.Price;
        existingProduct.StockQuantity = updateProductDto.StockQuantity;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return updatedProduct != null ? MapToDto(updatedProduct) : null;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchCriteria criteria)
    {
        var (items, totalCount) = await _productRepository.SearchAsync(
            criteria.Keyword,
            criteria.MinPrice,
            criteria.MaxPrice,
            criteria.InStock,
            criteria.Page,
            criteria.PageSize,
            criteria.Sort);

        var productDtos = items.Select(MapToDto).ToList();
        return new PagedResult<ProductDto>(productDtos, totalCount, criteria.Page, criteria.PageSize);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
