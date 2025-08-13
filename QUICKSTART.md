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
- Or use the provided `api-test.http` file for quick testing

## Sample API Calls

### Create a Product
```bash
curl -X POST "https://localhost:7042/api/products" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Gaming Laptop",
       "description": "High-performance gaming laptop",
       "category": "Electronics", 
       "price": 1999.99,
       "stockQuantity": 25
     }'
```

### Get All Products
```bash
curl -X GET "https://localhost:7042/api/products"
```

### Search Products
```bash
curl -X GET "https://localhost:7042/api/products?keyword=laptop&minPrice=1000"
```

## Troubleshooting

### Database Issues
- Ensure SQL Server/LocalDB is running
- Check connection string in `appsettings.json`
- Verify EF migrations were applied

### Build Issues
- Run `dotnet restore` to restore packages
- Check all project references are correct
- Ensure .NET 8 SDK is installed

### Port Issues
- Default API port is 7042 (HTTPS) and 5042 (HTTP)
- Update `launchSettings.json` if ports are in use

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

## Next Steps
1. Explore the Swagger documentation
2. Run the unit tests: `dotnet test`
3. Try the sample HTTP requests in `api-test.http`
4. Review the code structure and clean architecture implementation

Happy coding! 🚀
