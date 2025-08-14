using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.DTOs;

namespace Inventory.UseCases.Services;

/// <summary>
/// Cached wrapper for ProductService that optimizes search operations with in-memory caching.
/// Implements the Decorator pattern to add caching functionality without modifying the core service.
/// </summary>
/// <remarks>
/// Cache Strategy:
/// - Only caches search operations (read-only)
/// - Uses 1-minute cache duration for balanced performance vs data freshness
/// - Cache keys are generated from search criteria hash for efficient lookup
/// - Write operations (Create/Update/Delete) invalidate related cache entries
/// </remarks>
public class CachedProductService : IProductService
{
    private readonly IProductService _productService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedProductService> _logger;
    
    // Cache configuration
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    private const string SearchCacheKeyPrefix = "product_search";
    private const string ProductCacheKeyPrefix = "product_item";

    /// <summary>
    /// Initializes a new instance of the cached product service.
    /// </summary>
    /// <param name="productService">The underlying product service to cache</param>
    /// <param name="cache">Memory cache instance for storing search results</param>
    /// <param name="logger">Logger for cache hit/miss tracking</param>
    public CachedProductService(
        IProductService productService, 
        IMemoryCache cache,
        ILogger<CachedProductService> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for products with caching optimization.
    /// Cache key is generated from search criteria to ensure proper cache isolation.
    /// </summary>
    /// <param name="criteria">Search criteria including filters, pagination, and sorting</param>
    /// <returns>Cached or fresh search results with pagination information</returns>
    /// <example>
    /// // This search will be cached for 1 minute
    /// var results = await cachedService.SearchProductsAsync(new ProductSearchCriteria 
    /// { 
    ///     Keyword = "laptop", 
    ///     Page = 1, 
    ///     PageSize = 20 
    /// });
    /// </example>
    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchCriteria criteria)
    {
        var cacheKey = GenerateSearchCacheKey(criteria);
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out PagedResult<ProductDto>? cachedResult) && cachedResult != null)
        {
            _logger.LogDebug("Cache HIT for search: {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogDebug("Cache MISS for search: {CacheKey}", cacheKey);

        // Get fresh data from underlying service
        var result = await _productService.SearchProductsAsync(criteria);
        
        // Cache the result with sliding expiration
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromSeconds(30), // Extend cache if accessed within 30 seconds
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, result, cacheOptions);
        
        _logger.LogDebug("Cached search result: {CacheKey} (Items: {ItemCount}, TotalCount: {TotalCount})", 
            cacheKey, result.Items.Count, result.TotalCount);

        return result;
    }

    /// <summary>
    /// Gets a single product by ID with caching.
    /// Individual products are cached separately from search results.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Cached or fresh product data</returns>
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

    /// <summary>
    /// Creates a new product and invalidates related cache entries.
    /// Cache invalidation ensures data consistency after write operations.
    /// </summary>
    /// <param name="createDto">Product creation data</param>
    /// <returns>Created product information</returns>
    public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
    {
        var result = await _productService.CreateProductAsync(createDto);
        
        // Invalidate search caches since new product affects search results
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product created and search caches invalidated: {ProductId}", result.Id);
        
        return result;
    }

    /// <summary>
    /// Updates an existing product and invalidates related cache entries.
    /// Both individual product cache and search caches are cleared.
    /// </summary>
    /// <param name="id">Product identifier to update</param>
    /// <param name="updateDto">Updated product data</param>
    /// <returns>Updated product information</returns>
    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateDto);
        
        // Invalidate specific product cache
        var productCacheKey = $"{ProductCacheKeyPrefix}:{id}";
        _cache.Remove(productCacheKey);
        
        // Invalidate search caches since product details changed
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product updated and caches invalidated: {ProductId}", id);
        
        return result;
    }

    /// <summary>
    /// Deletes a product and invalidates all related cache entries.
    /// Ensures deleted products don't appear in cached search results.
    /// </summary>
    /// <param name="id">Product identifier to delete</param>
    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        // Invalidate specific product cache
        var productCacheKey = $"{ProductCacheKeyPrefix}:{id}";
        _cache.Remove(productCacheKey);
        
        // Invalidate search caches since product was deleted
        InvalidateSearchCaches();
        
        _logger.LogDebug("Product deleted and caches invalidated: {ProductId}", id);
        
        return result;
    }

    /// <summary>
    /// Generates a unique cache key for search criteria.
    /// Hash-based approach ensures efficient lookup while handling complex criteria.
    /// </summary>
    /// <param name="criteria">Search criteria to generate key from</param>
    /// <returns>Unique cache key string</returns>
    private static string GenerateSearchCacheKey(ProductSearchCriteria criteria)
    {
        // Create a deterministic hash from search criteria
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

    /// <summary>
    /// Invalidates all search-related cache entries.
    /// Called after write operations to maintain data consistency.
    /// </summary>
    /// <remarks>
    /// This is a simplified approach that clears all search caches.
    /// In production, you might implement more granular cache invalidation
    /// based on specific categories or price ranges affected.
    /// </remarks>
    private void InvalidateSearchCaches()
    {
        // Note: MemoryCache doesn't have a direct way to remove by prefix
        // In production, consider using a more sophisticated caching solution
        // like Redis with key pattern matching for selective invalidation
        
        if (_cache is MemoryCache memoryCache)
        {
            // Access the private field to get all cache keys (reflection-based approach)
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
