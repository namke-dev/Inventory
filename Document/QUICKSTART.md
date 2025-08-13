# Inventory Management System - Quick Start Guide
## Prerequisites
- .NET 8 SDK installed
- SQL Server or LocalDB available

### 1. Install EF Core Tools
```bash
dotnet tool install --global dotnet-ef
```

### 2. Create the Database
```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project Inventory.Infrastructure --startup-project Inventory.API

# Update the database
dotnet ef database update --project Inventory.Infrastructure --startup-project Inventory.API
```

### 3. Run the Application
```bash
# Start the API
dotnet run --project Inventory.API
```

### 4. Test the API
- Open your browser and navigate to: `https://localhost:7042/swagger`
- Use the provided `api-test.http` file for quick testing

## Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐
│   Inventory.API │────│ Inventory.      │
│   Controllers   │    │ UseCases        │
└─────────────────┘    └─────────────────┘
                              │
                       ┌─────────────────┐
                       │ Inventory.      │
                       │ Infrastructure  │
                       └─────────────────┘
                              │
                       ┌─────────────────┐
                       │ Inventory.      │
                       │ Domain          │
                       └─────────────────┘
```