# Order Processing System

## Overview
A .NET 10.0 Backend Service for processing e-commerce orders, built with **Clean Architecture** principles.

## Features
- **Create Order**: Place orders with multiple items (Validated via **FluentValidation**).
- **Get Order**: Retrieve details by ID.
- **List Orders**: Filter by status.
- **Cancel Order**: Cancel Pending orders only.
- **Background Job**: Automatically moves `Pending` orders to `Processing` every 5 minutes.
- **Logging**: Structured logging with **Serilog** (Console + File).
- **Error Handling**: Global Exception Middleware + Standardized JSON responses.

## How to Run
1. **Prerequisites**: Ensure [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) is installed.
2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```
3. **Run the API**:
   ```bash
   dotnet run --project src/OrderProcessingSystem.Api
   ```
   The API will be available at `https://localhost:55837/swagger`.

4. **Run Tests**:
   ```bash
   dotnet test
   ```

5. **Run with Docker**:
   ```bash
   # Build the image
   docker build -t order-api .

   # Run container
   docker run -p 5000:80 order-api
   ```
   Access at `http://localhost:5000/swagger`.

## Architecture
The solution follows **Clean Architecture** (Onion Architecture):
- **Domain**: Core entities (`Order`, `OrderItem`) and business rules. No external dependencies.
- **Application**: Business logic (`OrderService`), Validators (`FluentValidation`), DTOs, and Interfaces.
- **Infrastructure**: Implementation of Interfaces (`OrderRepository`, `AppDbContext`) and Background Services.
- **Api**: REST Controllers, Serilog Logging, and Exception Middleware.

## Design Patterns Used
- **Repository Pattern**: To decouple business logic from data access (`IOrderRepository`).
- **Dependency Injection**: To manage dependencies and lifecycle.
- **Hosted Service**: For background processing (`OrderStatusUpdaterService`).
- **DTO Pattern**: To separate API contracts from Domain entities.
- **Middleware**: Global Exception Handling transparently manages errors.

## AI Usage Report
**Tool Used**: Cursor AI / Deepmind Agent
**Purpose**:
- **Scaffolding**: Generated Clean Architecture Solution structure.
- **Refactoring**: Converted basic DataAnnotations to **FluentValidation** for cleanly separated rules.
- **Quality Assurance**: Implemented **Serilog** for production-grade logging and Global Exception Handling.
- **Migration**: Automatically migrated project from .NET 8.0 to .NET 10.0 based on environment detection.
