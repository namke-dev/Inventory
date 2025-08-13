#!/bin/bash

# Inventory Management System - Complete Setup Script
# This script creates a clean architecture .NET 8 solution for inventory management

echo "🚀 Setting up Inventory Management System..."

# Create solution root folder and enter it
mkdir InventoryManagement
cd InventoryManagement

echo "📁 Creating solution and projects..."

# Create a new solution file
dotnet new sln -n InventoryManagement

# Create projects with clean architecture structure
dotnet new webapi -n Inventory.API
dotnet new classlib -n Inventory.UseCases       # Application layer
dotnet new classlib -n Inventory.Domain         # Core domain layer
dotnet new classlib -n Inventory.Infrastructure # Data access layer
dotnet new xunit -n Inventory.Tests             # Unit tests

echo "🔗 Adding projects to solution..."

# Add all projects to the solution
dotnet sln add Inventory.API/Inventory.API.csproj
dotnet sln add Inventory.UseCases/Inventory.UseCases.csproj
dotnet sln add Inventory.Domain/Inventory.Domain.csproj
dotnet sln add Inventory.Infrastructure/Inventory.Infrastructure.csproj
dotnet sln add Inventory.Tests/Inventory.Tests.csproj

echo "🔧 Setting up project references..."

# Add project references following clean architecture dependency rules

# API layer depends on UseCases and Infrastructure
dotnet add Inventory.API/Inventory.API.csproj reference Inventory.UseCases/Inventory.UseCases.csproj
dotnet add Inventory.API/Inventory.API.csproj reference Inventory.Infrastructure/Inventory.Infrastructure.csproj

# UseCases depends only on Domain
dotnet add Inventory.UseCases/Inventory.UseCases.csproj reference Inventory.Domain/Inventory.Domain.csproj

# Infrastructure depends on Domain and UseCases (for interfaces)
dotnet add Inventory.Infrastructure/Inventory.Infrastructure.csproj reference Inventory.Domain/Inventory.Domain.csproj
dotnet add Inventory.Infrastructure/Inventory.Infrastructure.csproj reference Inventory.UseCases/Inventory.UseCases.csproj

# Tests can reference all layers for comprehensive testing
dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.UseCases/Inventory.UseCases.csproj
dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.Infrastructure/Inventory.Infrastructure.csproj
dotnet add Inventory.Tests/Inventory.Tests.csproj reference Inventory.Domain/Inventory.Domain.csproj

echo "📦 Installing NuGet packages..."

# Infrastructure layer packages - Entity Framework Core for SQL Server
dotnet add Inventory.Infrastructure/Inventory.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.8
dotnet add Inventory.Infrastructure/Inventory.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools --version 9.0.8

# UseCases layer packages - FluentValidation for input validation
dotnet add Inventory.UseCases/Inventory.UseCases.csproj package FluentValidation --version 12.0.0

# API layer packages - EF Core Design (for migrations), Serilog for logging, OpenAPI/Swagger
dotnet add Inventory.API/Inventory.API.csproj package Microsoft.EntityFrameworkCore.Design --version 9.0.8
dotnet add Inventory.API/Inventory.API.csproj package Serilog.AspNetCore --version 9.0.0
dotnet add Inventory.API/Inventory.API.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.19
dotnet add Inventory.API/Inventory.API.csproj package Swashbuckle.AspNetCore --version 6.6.2

# Test layer packages - xUnit framework, Moq for mocking, test SDK
dotnet add Inventory.Tests/Inventory.Tests.csproj package Moq --version 4.20.72
dotnet add Inventory.Tests/Inventory.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add Inventory.Tests/Inventory.Tests.csproj package xunit --version 2.5.3
dotnet add Inventory.Tests/Inventory.Tests.csproj package xunit.runner.visualstudio --version 2.5.3
dotnet add Inventory.Tests/Inventory.Tests.csproj package coverlet.collector --version 6.0.0

echo "🛠️ Installing EF Core global tools..."

# Install Entity Framework Core global tool for migrations
dotnet tool install --global dotnet-ef

echo "📋 Dependency Flow Summary:"
echo "┌─────────────────┐    ┌─────────────────┐"
echo "│   Inventory.API │────│ Inventory.      │"
echo "│   Controllers   │    │ UseCases        │"
echo "└─────────────────┘    └─────────────────┘"
echo "                              │"
echo "                       ┌─────────────────┐"
echo "                       │ Inventory.      │"
echo "                       │ Infrastructure  │"
echo "                       └─────────────────┘"
echo "                              │"
echo "                       ┌─────────────────┐"
echo "                       │ Inventory.      │"
echo "                       │ Domain          │"
echo "                       └─────────────────┘"

echo ""
echo "✅ Solution setup complete!"
echo ""