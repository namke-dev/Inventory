using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Serilog;
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
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add services
builder.Services.AddScoped<IProductService, ProductService>();

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
    app.UseSwaggerUI();
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
