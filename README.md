# Inventory Management System

A high-performance, clean architecture .NET 8 Web API for managing product inventory with advanced search capabilities, CRUD operations, and enterprise-grade features.

## 🏗️ **System Architecture**

This project implements **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns, ensuring maintainability, testability, and scalability.

### **📊 Architecture Overview**

```
┌─────────────────────────────────────────────────────────────────┐
│                        🌐 PRESENTATION LAYER                    │
├─────────────────────────────────────────────────────────────────┤
│  🎮 Inventory.API (ASP.NET Core Web API)                      │
│  ├── Controllers (REST Endpoints)                              │
│  ├── Middleware (Logging, CORS, Exception Handling)           │
│  ├── Dependency Injection (Service Registration)              │
│  └── Configuration (Swagger, Authentication, Validation)      │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                      💼 APPLICATION LAYER                       │
├─────────────────────────────────────────────────────────────────┤
│  🔧 Inventory.UseCases (Business Logic)                       │
│  ├── Services (Product Management Logic)                       │
│  ├── DTOs (Data Transfer Objects)                             │
│  ├── Interfaces (Repository & Service Contracts)              │
│  └── Validators (FluentValidation Rules)                      │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                     🗄️ INFRASTRUCTURE LAYER                     │
├─────────────────────────────────────────────────────────────────┤
│  🏪 Inventory.Infrastructure (Data Access)                    │
│  ├── EF Core DbContext (Database Configuration)               │
│  ├── Repositories (Data Access Implementation)                │
│  ├── Migrations (Database Schema Management)                  │
│  └── Database Indexes (Performance Optimization)             │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                        🏛️ DOMAIN LAYER                          │
├─────────────────────────────────────────────────────────────────┤
│  📦 Inventory.Domain (Core Business Logic)                    │
│  ├── Entities (Product, Business Rules)                       │
│  ├── Value Objects (Domain Primitives)                        │
│  ├── Domain Services (Core Business Logic)                    │
│  └── Domain Events (Business Events)                          │
└─────────────────────────────────────────────────────────────────┘
```

### **🔄 Data Flow Architecture**

```
📱 Client Request
    │
    ▼
🌐 API Controller (Validation, Logging)
    │
    ▼
💼 Application Service (Business Logic)
    │
    ▼
🗄️ Repository Interface (Data Abstraction)
    │
    ▼
🏪 EF Core Repository (Database Operations)
    │
    ▼
📊 SQL Server Database (Optimized with Indexes)
    │
    ▼
📤 Response (DTO → JSON)
```

### **📐 Clean Architecture Benefits**

| Principle | Implementation | Benefit |
|-----------|----------------|---------|
| **Dependency Inversion** | Interfaces in UseCases, Implementations in Infrastructure | Easy testing & mocking |
| **Single Responsibility** | Each layer has distinct concerns | Maintainable code |
| **Open/Closed** | Extension through interfaces | Easy feature additions |
| **Interface Segregation** | Focused repository interfaces | Clean contracts |
| **Dependency Rule** | Dependencies point inward only | Stable architecture |

## 🏛️ **Layer Responsibilities**

### **🏛️ Domain Layer (`Inventory.Domain`)**
```csharp
// Pure business logic, no external dependencies
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }     // Business rule: Required, max 200 chars
    public decimal Price { get; set; }   // Business rule: Must be positive
    public int StockQuantity { get; set; } // Business rule: Non-negative
    // ... domain logic
}
```

**Responsibilities:**
- 📋 Core business entities and rules
- 🔒 Domain validation and constraints  
- 🚫 **Zero external dependencies**
- ⚡ Pure C# with business logic only

### **💼 Application Layer (`Inventory.UseCases`)**
```csharp
// Orchestrates business workflows
public class ProductService : IProductService
{
    // Coordinates domain operations
    // Validates input using FluentValidation
    // Maps between DTOs and domain entities
    // Enforces business workflows
}
```

**Responsibilities:**
- 🔧 Business workflow orchestration
- ✅ Input validation with FluentValidation
- 🔄 DTO mapping and transformation
- 📐 Service interfaces and contracts
- **Dependencies:** Domain layer only

### **🗄️ Infrastructure Layer (`Inventory.Infrastructure`)**
```csharp
// Database implementation details
public class ProductRepository : IProductRepository
{
    // EF Core database operations
    // Optimized queries with indexes
    // Advanced search implementations
    // Database-specific logic
}
```

**Responsibilities:**
- 🏪 Database access with EF Core
- 📊 Query optimization and indexing
- 🔄 Data persistence and retrieval
- 🗃️ Migration and schema management
- **Dependencies:** Domain + UseCases

### **🌐 Presentation Layer (`Inventory.API`)**
```csharp
// HTTP API endpoints
[ApiController]
public class ProductsController : ControllerBase
{
    // REST endpoint implementations
    // HTTP status code management
    // Request/response handling
    // Error handling and logging
}
```

**Responsibilities:**
- 🌐 REST API endpoint definitions
- 📨 HTTP request/response handling
- 🚨 Error handling and logging
- 📚 API documentation (Swagger)
- **Dependencies:** UseCases + Infrastructure

### Test Layer (`Inventory.Tests`)
- **Unit Tests**: Comprehensive testing with Moq
- Tests service layer logic and validation
- Depends on all core layers

## Features

### Product Management
- ✅ Create new products
- ✅ Retrieve product by ID
- ✅ Update existing products
- ✅ Delete products
- ✅ Search products with filters
- ✅ Pagination support

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
- `sort`: Sort field (name, price, created, stock)

## 📚 **Documentation**

- 📖 **[Architecture Guide](ARCHITECTURE.md)** - Detailed system architecture and design patterns
- ⚡ **[Performance Guide](PERFORMANCE.md)** - Performance optimizations and benchmarks  
- 🚀 **[Quick Start Guide](Document/QUICKSTART.md)** - Get up and running in minutes
- 🧪 **[API Testing](Document/api-test.http)** - Ready-to-use HTTP requests for testing

## 🛠️ **Technology Stack**

- **.NET 8**: Latest LTS version
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM with SQL Server
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or LocalDB
- Visual Studio 2022 / VS Code

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd InventoryManagement
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   - Update connection string in `appsettings.json`
   - Default uses LocalDB: `(localdb)\\mssqllocaldb`

4. **Create database**
   ```bash
   dotnet ef database update --project Inventory.Infrastructure --startup-project Inventory.API
   ```

5. **Run the application**
   ```bash
   dotnet run --project Inventory.API
   ```

6. **Access Swagger UI**
   - Navigate to: `https://localhost:7XXX/swagger`
   - Explore and test API endpoints

### Running Tests
```bash
dotnet test
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

## Configuration

### Database
- **Provider**: SQL Server
- **Connection String**: Configured in appsettings.json
- **Migrations**: EF Core migrations for schema management

### Logging
- **Serilog**: Structured logging to console and file
- **Log Files**: Stored in `logs/` directory
- **Rolling**: Daily log file rotation

### CORS
- **Development**: Allow all origins
- **Production**: Configure specific origins

## API Examples

### Create Product
```json
POST /api/products
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "category": "Electronics",
  "price": 999.99,
  "stockQuantity": 50
}
```

### Search Products
```
GET /api/products?keyword=laptop&minPrice=500&maxPrice=2000&page=1&pageSize=10&sort=price
```

### Response Format
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Laptop",
      "description": "High-performance laptop",
      "category": "Electronics",
      "price": 999.99,
      "stockQuantity": 50,
      "createdAt": "2025-08-13T10:00:00Z",
      "updatedAt": "2025-08-13T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## Development Guidelines

### Clean Architecture Principles
1. **Dependency Rule**: Dependencies point inward toward the domain
2. **Interface Segregation**: Use specific interfaces for different concerns
3. **Single Responsibility**: Each class has one reason to change
4. **Testability**: Business logic is easily testable

### Code Quality
- **Validation**: All inputs validated using FluentValidation
- **Error Handling**: Comprehensive exception handling
- **Logging**: Structured logging for monitoring
- **Testing**: High test coverage with unit tests

## Contributing

1. Follow Clean Architecture principles
2. Write comprehensive unit tests
3. Use FluentValidation for input validation
4. Include XML documentation for API endpoints
5. Follow existing code style and patterns

## License

This project is licensed under the MIT License.
