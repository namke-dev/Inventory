using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;
using Inventory.UseCases.Interfaces;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Product> CreateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(IList<Product> Items, int TotalCount)> SearchAsync(
        string? keyword,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        int page,
        int pageSize,
        string sort)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            query = query.Where(p => 
                EF.Functions.Like(p.Name.ToLower(), $"%{keywordLower}%") || 
                EF.Functions.Like(p.Description.ToLower(), $"%{keywordLower}%") || 
                EF.Functions.Like(p.Category.ToLower(), $"%{keywordLower}%"));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (inStock.HasValue)
        {
            if (inStock.Value)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }
            else
            {
                query = query.Where(p => p.StockQuantity == 0);
            }
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            query = query.OrderBy(p => 
                // Exact name match gets priority 1
                p.Name.ToLower() == keywordLower ? 1 :
                // Name starts with keyword gets priority 2  
                p.Name.ToLower().StartsWith(keywordLower) ? 2 :
                // Category exact match gets priority 3
                p.Category.ToLower() == keywordLower ? 3 :
                // Category starts with keyword gets priority 4
                p.Category.ToLower().StartsWith(keywordLower) ? 4 :
                // Other matches get priority 5
                5)
            .ThenBy(p => p.Name); // Secondary sort by name
        }
        else
        {
            query = sort.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "created" => query.OrderBy(p => p.CreatedAt),
                "created_desc" => query.OrderByDescending(p => p.CreatedAt),
                "stock" => query.OrderBy(p => p.StockQuantity),
                "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
                _ => query.OrderBy(p => p.Name)
            };
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}


