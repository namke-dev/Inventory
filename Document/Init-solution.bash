# Create solution root folder and enter it
mkdir InventoryManagement
cd InventoryManagement

# Create a new solution file
dotnet new sln -n InventoryManagement

# Create projects
dotnet new webapi -n Inventory.API
dotnet new classlib -n Inventory.UseCases
dotnet new classlib -n Inventory.Domain
dotnet new classlib -n Inventory.Infrastructure
dotnet new xunit -n Inventory.Tests

# Add all projects to the solution
dotnet sln add Inventory.API/Inventory.API.csproj
dotnet sln add Inventory.UseCases/Inventory.UseCases.csproj
dotnet sln add Inventory.Domain/Inventory.Domain.csproj
dotnet sln add Inventory.Infrastructure/Inventory.Infrastructure.csproj
dotnet sln add Inventory.Tests/Inventory.Tests.csproj

# Add project references
dotnet add Inventory.API/Inventory.API.csproj reference Inventory.UseCases/Inventory.UseCases.csproj
dotnet add Inventory.API/Inventory.API.csproj reference Inventory.Infrastructure/Inventory.Infrastructure.csproj

dotnet add Inventory.UseCases/Inventory.UseCases.csproj reference Inventory.Domain/Inventory.Domain.csproj
dotnet add Inventory.Infrastructure/Inventory.Infrastructure.csproj reference Inventory.Domain/Inventory.Domain.csproj

dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.UseCases/Inventory.UseCases.csproj
dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.Infrastructure/Inventory.Infrastructure.csproj
dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.Domain/Inventory.Domain.csproj


# Dependency flow:
# API → UseCases & Infrastructure
# UseCases → Domain
# Infrastructure → Domain
# Tests → All core layers