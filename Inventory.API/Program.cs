using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FluentValidation;
using Serilog;
using System.Reflection;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Inventory.UseCases.Interfaces;
using Inventory.UseCases.Services;
using Inventory.UseCases.DTOs;
using Inventory.UseCases.Validators;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/inventory-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory Management API",
        Version = "v1",
        Description = "A Inventory management system for retail stores optimized for product search, with enhanced features"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    var useCasesXmlFile = "Inventory.UseCases.xml";
    var useCasesXmlPath = Path.Combine(AppContext.BaseDirectory, useCasesXmlFile);
    if (File.Exists(useCasesXmlPath))
    {
        c.IncludeXmlComments(useCasesXmlPath);
    }
});

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add services with caching layer
builder.Services.AddScoped<ProductService>(); // Register the base service as itself
builder.Services.AddScoped<IProductService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<ProductService>();
    var cache = serviceProvider.GetRequiredService<IMemoryCache>();
    var logger = serviceProvider.GetRequiredService<ILogger<CachedProductService>>();
    return new CachedProductService(baseService, cache, logger);
});

// Add validators
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API v1");
        c.RoutePrefix = string.Empty;
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting up the application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}


