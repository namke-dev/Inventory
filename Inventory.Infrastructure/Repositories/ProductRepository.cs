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
        _context = context;
    }

    public async Task<Product> CreateAsync(Product product)
    {
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
        var query = _context.Products.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword) || 
                                    p.Description.Contains(keyword) || 
                                    p.Category.Contains(keyword));
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

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
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

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
