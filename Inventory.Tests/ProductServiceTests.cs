using Moq;
using Xunit;
using FluentValidation;
using Inventory.Domain.Entities;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.Services;
using Inventory.UseCases.DTOs;
using Inventory.UseCases.Validators;

namespace Inventory.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductService _productService;
    private readonly CreateProductDtoValidator _createValidator;
    private readonly UpdateProductDtoValidator _updateValidator;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _createValidator = new CreateProductDtoValidator();
        _updateValidator = new UpdateProductDtoValidator();
        _productService = new ProductService(_mockRepository.Object, _createValidator, _updateValidator);
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsProductDto()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.99m,
            StockQuantity = 100
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            Category = createDto.Category,
            Price = createDto.Price,
            StockQuantity = createDto.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                      .ReturnsAsync(product);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.Category, result.Category);
        Assert.Equal(createDto.Price, result.Price);
        Assert.Equal(createDto.StockQuantity, result.StockQuantity);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_InvalidProduct_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "", // Invalid: empty name
            Description = "Test Description",
            Category = "Test Category",
            Price = -1, // Invalid: negative price
            StockQuantity = 100
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _productService.CreateProductAsync(createDto));
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ExistingProduct_ReturnsUpdatedProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Category = "Old Category",
            Price = 5.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new UpdateProductDto
        {
            Name = "New Name",
            Description = "New Description",
            Category = "New Category",
            Price = 15.99m,
            StockQuantity = 75
        };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(existingProduct);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                      .ReturnsAsync((Product p) => p);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.Category, result.Category);
        Assert.Equal(updateDto.Price, result.Price);
        Assert.Equal(updateDto.StockQuantity, result.StockQuantity);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(productId))
                      .ReturnsAsync(true);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_NonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(productId))
                      .ReturnsAsync(false);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task SearchProductsAsync_ValidCriteria_ReturnsPagedResult()
    {
        // Arrange
        var criteria = new ProductSearchCriteria
        {
            Keyword = "test",
            Page = 1,
            PageSize = 10
        };

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Test Product 1", Category = "Category1", Price = 10.99m, StockQuantity = 10 },
            new Product { Id = Guid.NewGuid(), Name = "Test Product 2", Category = "Category2", Price = 15.99m, StockQuantity = 20 }
        };

        _mockRepository.Setup(r => r.SearchAsync(
            criteria.Keyword,
            criteria.MinPrice,
            criteria.MaxPrice,
            criteria.InStock,
            criteria.Page,
            criteria.PageSize,
            criteria.Sort))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _productService.SearchProductsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        _mockRepository.Verify(r => r.SearchAsync(
            criteria.Keyword,
            criteria.MinPrice,
            criteria.MaxPrice,
            criteria.InStock,
            criteria.Page,
            criteria.PageSize,
            criteria.Sort), Times.Once);
    }
}