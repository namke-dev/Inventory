# Inventory Management System

A clean architecture .NET 8 Web API for managing product inventory with CRUD operations, search functionality, and pagination.

## Architecture

This project follows Clean Architecture principles with the following layers:

### Domain Layer (`Inventory.Domain`)
- **Product Entity**: Core business entity representing a product in the inventory
- Contains business rules and domain logic
- No dependencies on external layers

### Application Layer (`Inventory.UseCases`)
- **Services**: Business logic implementation
- **DTOs**: Data Transfer Objects for API communication
- **Interfaces**: Abstractions for dependency inversion
- **Validators**: FluentValidation rules for input validation
- Depends only on Domain layer

### Infrastructure Layer (`Inventory.Infrastructure`)
- **Entity Framework DbContext**: Database access using EF Core
- **Repository Implementation**: Data access layer
- **Database Configuration**: Entity configurations and mappings
- Depends on Domain and UseCases layers

### Presentation Layer (`Inventory.API`)
- **Controllers**: RESTful API endpoints
- **Dependency Injection**: Service registration
- **Configuration**: Logging, CORS, Swagger
- Depends on UseCases and Infrastructure layers

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

## Technology Stack

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
