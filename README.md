# LogiTrack - Order Management System

LogiTrack is a logistics software platform that manages inventory items and customer orders across multiple fulfillment centers.

## Project Overview

This is the first part of a five-part capstone project that implements:

- **Business Logic Classes**: InventoryItem and Order models
- **Database Integration**: Entity Framework Core with SQLite
- **CRUD Operations**: Create, view, and update inventory and orders
- **Performance Optimization**: Proper EF Core relationships and indexing

## Features

### InventoryItem Class
- Properties: ItemId, Name, Quantity, Location
- Methods: DisplayInfo(), ToString() override
- Data Annotations: Required fields, indexing for performance

### Order Class  
- Properties: OrderId, CustomerName, DatePlaced
- Navigation Properties: OrderItems collection for proper EF Core relationships
- Methods: AddItem(), RemoveItem(), GetOrderSummary(), PrintOrderDetails()
- Performance Features: GetTotalQuantity() with LINQ optimization

### OrderItem Class
- Join entity for proper many-to-many relationship between Orders and InventoryItems
- Properties: OrderItemId, OrderId, InventoryItemId, QuantityOrdered
- Navigation Properties: Order, InventoryItem

## Technology Stack

- **Framework**: ASP.NET Core 9.0 Web API
- **Database**: SQLite with Entity Framework Core
- **Language**: C# 
- **Tools**: Entity Framework Tools, .NET CLI

## Database Schema

The application uses Entity Framework Code First approach with:
- Proper foreign key relationships
- Indexes on Name and Location fields for performance
- Data annotations for validation and constraints

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Entity Framework Tools (`dotnet tool install --global dotnet-ef`)

### Running the Application

1. Clone the repository
2. Navigate to the LogiTrack directory
3. Restore packages: `dotnet restore`
4. Run migrations: `dotnet ef database update`
5. Run the application: `dotnet run`

### Sample Output

```
Current Inventory:
Item: Pallet Jack | Quantity: 12 | Location: Warehouse A

Order Summary:
Order #1 for Samir | Items: 1 | Placed: 4/5/2025
Order Details:
  - Pallet Jack: 2 units from Warehouse A
Total Quantity: 2
```

## Development Process

This project was developed following best practices:

1. **Model-First Design**: Created business logic classes first
2. **Database Integration**: Added EF Core with proper relationships  
3. **Testing**: Comprehensive testing with sample data
4. **Optimization**: Applied performance improvements and code review suggestions
5. **Documentation**: Clear code documentation and README

## Future Enhancements

This foundational implementation will be extended in upcoming project phases to include:
- API endpoints for external integration
- Security and authentication
- Advanced performance optimization
- Comprehensive error handling

## Contributing

This is a capstone project for educational purposes. The codebase demonstrates:
- Clean architecture principles
- Entity Framework best practices  
- Proper Git workflow and documentation
- Test-driven development approach

## License

This project is for educational purposes as part of a DevOps capstone project.