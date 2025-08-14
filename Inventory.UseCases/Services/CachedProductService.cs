using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.DTOs;

namespace Inventory.UseCases.Services;
public class CachedProductService : IProductService
{
    private readonly IProductService _productService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedProductService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    private const string SearchCacheKeyPrefix = "product_search";
    private const string ProductCacheKeyPrefix = "product_item";
    public CachedProductService(
        IProductService productService, 
        IMemoryCache cache,
        ILogger<CachedProductService> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchCriteria criteria)
    {
        var cacheKey = GenerateSearchCacheKey(criteria);
        if (_cache.TryGetValue(cacheKey, out PagedResult<ProductDto>? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("🎯 CACHE HIT for search: {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogInformation("❌ CACHE MISS for search: {CacheKey}", cacheKey);
        var result = await _productService.SearchProductsAsync(criteria);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromSeconds(30),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, result, cacheOptions);
        
        _logger.LogInformation("✅ CACHED search result: {CacheKey} (Items: {ItemCount}, TotalCount: {TotalCount})", 
            cacheKey, result.Items.Count, result.TotalCount);

        return result;
    }
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}:{id}";
        
        if (_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct) && cachedProduct != null)
        {
            _logger.LogDebug("Cache HIT for product: {ProductId}", id);
            return cachedProduct;
        }

        _logger.LogDebug("Cache MISS for product: {ProductId}", id);

        var product = await _productService.GetProductByIdAsync(id);
        
        if (product != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, product, cacheOptions);
            _logger.LogDebug("Cached product: {ProductId}", id);
        }

        return product;
    }
    public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
    {
        var result = await _productService.CreateProductAsync(createDto);
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product created and search caches invalidated: {ProductId}", result.Id);
        
        return result;
    }
    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateDto);
        var productCacheKey = $"{ProductCacheKeyPrefix}:{id}";
        _cache.Remove(productCacheKey);
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product updated and caches invalidated: {ProductId}", id);
        
        return result;
    }
    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        var productCacheKey = $"{ProductCacheKeyPrefix}:{id}";
        _cache.Remove(productCacheKey);
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product deleted and caches invalidated: {ProductId}", id);
        
        return result;
    }
    private static string GenerateSearchCacheKey(ProductSearchCriteria criteria)
    {
        var keyData = new
        {
            Keyword = criteria.Keyword?.ToLowerInvariant(),
            MinPrice = criteria.MinPrice,
            MaxPrice = criteria.MaxPrice,
            InStock = criteria.InStock,
            Page = criteria.Page,
            PageSize = criteria.PageSize,
            Sort = criteria.Sort?.ToLowerInvariant()
        };

        var json = JsonSerializer.Serialize(keyData);
        var hash = json.GetHashCode();
        
        return $"{SearchCacheKeyPrefix}:{hash:X}";
    }
    private void InvalidateSearchCaches()
    {
        
        if (_cache is MemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is IDictionary<object, object> coherentState)
            {
                var keysToRemove = new List<object>();
                
                foreach (var entry in coherentState)
                {
                    if (entry.Key.ToString()?.StartsWith(SearchCacheKeyPrefix) == true)
                    {
                        keysToRemove.Add(entry.Key);
                    }
                }
                
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
                
                _logger.LogDebug("Invalidated {Count} search cache entries", keysToRemove.Count);
            }
        }
    }
}


