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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/inventory-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with XML documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory Management API",
        Version = "v1",
        Description = "A comprehensive inventory management system for retail stores",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Inventory Management Team",
            Email = "support@inventorymanagement.com"
        }
    });

    // Include XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Include XML comments from other assemblies (if they have XML docs enabled)
    var useCasesXmlFile = "Inventory.UseCases.xml";
    var useCasesXmlPath = Path.Combine(AppContext.BaseDirectory, useCasesXmlFile);
    if (File.Exists(useCasesXmlPath))
    {
        c.IncludeXmlComments(useCasesXmlPath);
    }
});

// Add Entity Framework with connection resilience
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        // Set command timeout for complex operations (30 seconds)
        sqlOptions.CommandTimeout(30);
        
        // Enable retry logic for transient failures
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,                      // Retry up to 3 times
            maxRetryDelay: TimeSpan.FromSeconds(5), // Maximum 5 second delay
            errorNumbersToAdd: null);               // Use default SQL error numbers
    }));

// Add Memory Caching for search optimization
builder.Services.AddMemoryCache();

// Add repositories
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

// Add CORS
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
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
