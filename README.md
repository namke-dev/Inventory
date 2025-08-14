# Inventory Management System

CLEAN architecture .NET 8 Web API for managing product inventory
Focus on hight performance search capabilities.

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

## Features

### Product Management
- CRUD products
- Search products with filters
- Pagination support

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

## 🛠️ **Technology Stack**

- **.NET 8**: Latest LTS version
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM with SQL Server
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework

