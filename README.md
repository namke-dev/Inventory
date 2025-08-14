# Inventory Management System

.NET 8 Web API for managing product inventory

## System Architecture

This project implements **Clean Architecture** principles

### Architecture Overview

```

                         PRESENTATION LAYER                    

   Inventory.API (ASP.NET Core Web API)                      
  ├── Controllers (REST Endpoints)                              
  ├── Middleware (Logging, CORS, Exception Handling)           
  ├── Dependency Injection (Service Registration)              
  └── Configuration (Swagger, Authentication, Validation)      
                                  
                                  ▼

                       APPLICATION LAYER                       

   Inventory.UseCases (Business Logic)                       
  ├── Services (Product Management Logic)                       
  ├── DTOs (Data Transfer Objects)                             
  ├── Interfaces (Repository & Service Contracts)              
  └── Validators (FluentValidation Rules)                      
                                  
                                  ▼

                      INFRASTRUCTURE LAYER                     

   Inventory.Infrastructure (Data Access)                    
  ├── EF Core DbContext (Database Configuration)               
  ├── Repositories (Data Access Implementation)                
  ├── Migrations (Database Schema Management)                  
  └── Database Indexes (Performance Optimization)             
                                  
                                  ▼

                         DOMAIN LAYER                          

   Inventory.Domain (Core Business Logic)                    
  ├── Entities (Product, Business Rules)                       
  ├── Value Objects (Domain Primitives)                        
  ├── Domain Services (Core Business Logic)                    
  └── Domain Events (Business Events)                          
```

### Dependency Flow (Inward Only)
```
┌─────────────┐      ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Presentation│───▶ │ Application  │───▶│Infrastructure│───▶│   Domain    │
│   (API)     │      │ (UseCases)  │     │  (Data)      │     │  (Entities) │
└─────────────┘      └─────────────┘     └──────────────┘     └─────────────┘
     HTTP              Business           Database           Business
   Endpoints             Logic             Access             Rules
```

## Project Structure

```
InventoryManagement/
├── Inventory.API/              # Web API layer
│   ├── Controllers/            # API controllers
│   ├── appsettings.json       # Configuration
│   └── Program.cs             # Application startup
├── Inventory.UseCases/         # Application layer
│   ├── DTOs/                  # Data transfer objects
│   ├── Interfaces/            # Service abstractions
│   ├── Services/              # Business logic
│   └── Validators/            # Input validation
├── Inventory.Domain/           # Domain layer
│   └── Entities/              # Domain entities
├── Inventory.Infrastructure/   # Infrastructure layer
│   ├── Data/                  # DbContext
│   └── Repositories/          # Data access
├── Inventory.Tests/            # Test layer
│   └── ProductServiceTests.cs # Unit tests
└── Document/
    └── Init-solution.bash     # Setup script
```

## Technology Stack

- **.NET 8**: Latest LTS version
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM with SQL Server
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework

## Features

- **CRUD Operations**: Create, Read, Update, Delete products
- **Filter Search**: Multi-criteria search with intelligent ranking
- **Pagination**: Efficient large dataset handling
- **Input Validation**: Comprehensive validation with detailed error messages
- **Performance Optimization**: Database indexing and query optimization

### API Endpoints

#### Products Controller (`/api/products`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/products` | Create a new product |
| GET | `/api/products/{id}` | Get product by ID |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| GET | `/api/products` | Search products with filters |

#### Search Parameters
- `keyword`: Search in name, description, category
- `minPrice`: Minimum price filter
- `maxPrice`: Maximum price filter
- `inStock`: Filter by stock availability
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20)
- `sort`: Sort field (name, price, created, stock) with _desc suffix for descending

## Performance Optimizations

### Database Performance

#### Strategic Indexing
```sql
-- High-impact indexes for search operations
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_Category ON Products(Category);
CREATE INDEX IX_Products_Price ON Products(Price);

-- Covering index for common search patterns
CREATE INDEX IX_Products_Search_Covering 
ON Products(Category, Price) 
INCLUDE (Name, StockQuantity);
```

#### Query Optimizations
```csharp
// 1. AsNoTracking for read-only operations
var query = _context.Products.AsNoTracking();

// 2. Efficient pagination
query.Skip((page - 1) * pageSize).Take(pageSize);

// 3. Field projection
var products = await query.Select(p => new ProductDto
{
    Id = p.Id,
    Name = p.Name,
    Price = p.Price,
    Category = p.Category
}).ToListAsync();
```

### Intelligent Search Relevance
When someone searches for "laptop", instead of random results, users get exactly what they expect:

1. Exact matches first - If they search "laptop", they want laptops
2. Related products next - Laptop accessories come after actual laptops
3. Similar categories follow - Products in laptop category
4. Everything else last - Products that just mention laptops

```csharp
// Multi-tier relevance scoring for keyword searches
query = query.OrderBy(p => 
    // Tier 1: Exact name match (highest relevance)
    p.Name.ToLower() == keywordLower ? 1 :
    // Tier 2: Name starts with keyword
    p.Name.ToLower().StartsWith(keywordLower) ? 2 :
    // Tier 3: Category exact match
    p.Category.ToLower() == keywordLower ? 3 :
    // Tier 4: Category starts with keyword
    p.Category.ToLower().StartsWith(keywordLower) ? 4 :
    // Tier 5: Other partial matches
    5)
.ThenBy(p => p.Name);
```

## Database Design

### Entity Model
```
┌─────────────────────────────────────┐
│              Products               │
├─────────────────────────────────────┤
│ [PK] Id (Guid, PK)                  │
│      Name (nvarchar(200), Indexed)  │
│      Description (nvarchar(500))    │
│      Category (nvarchar(100), Indexed) │
│      Price (decimal(18,2), Indexed) │
│      StockQuantity (int)            │
│      CreatedAt (datetime2)          │
│      UpdatedAt (datetime2)          │
└─────────────────────────────────────┘
```

### Business Constraints
```sql
-- Business rule enforcement at database level
ALTER TABLE Products 
  ADD CONSTRAINT CK_Products_Price_Positive 
  CHECK (Price > 0);

ALTER TABLE Products 
  ADD CONSTRAINT CK_Products_Stock_NonNegative 
  CHECK (StockQuantity >= 0);
```

## Security & Quality

### Input Validation
```csharp
// Multi-layer validation with FluentValidation
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);
    }
}
```

### Error Handling
- **Safe Error Responses**: No sensitive information leakage
- **Structured Logging**: Comprehensive operation tracking
- **Exception Management**: Centralized error handling
- **Request Tracing**: Correlation IDs for debugging

### Audit Trail
- **Creation Tracking**: Automatic CreatedAt timestamps
- **Modification Tracking**: UpdatedAt timestamps
- **Operation Logging**: Structured logging for all operations

## Scalability & Future Enhancements

### Current Architecture
```
Single Database Tier:
┌─────────┐     ┌─────────────┐     ┌─────────────┐
│   API   │───▶│ Application  │───▶│ SQL Server  │
│ Server  │     │   Logic     │     │  Database   │
└─────────┘     └─────────────┘     └─────────────┘
```

### Future Horizontal Scaling
```
Multi-Tier Architecture:
┌─────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│Load     │     │   API       │     │ Application │     │Read/Write   │
│Balancer │───▶│ Servers      │───▶│   Logic     │───▶│Split         │
│         │     │(Multiple)   │     │             │     │Database     │
└─────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

### Technology Upgrade Path

| Current | Next Level | Enterprise Level |
|---------|------------|------------------|
| SQL Server | SQL Server + Redis | CosmosDB + Redis Cluster |
| Single API | Multiple APIs | Microservices Architecture |
| EF Core | EF Core + Dapper | CQRS + Event Sourcing |
| File Logging | Structured Logging | Distributed Tracing (Jaeger) |

## Testing

### Testing Strategy
- **Unit Tests**: Business logic validation
- **Integration Tests**: Database operations
- **Performance Tests**: Search optimization validation
- **API Tests**: End-to-end workflow testing

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Setup
1. Clone the repository
2. Run `dotnet restore` to restore dependencies
3. Update connection string in `appsettings.json`
4. Run `dotnet ef database update` to apply migrations
5. Run `dotnet run` to start the API
6. Visit `http://localhost:5078` for Swagger documentation