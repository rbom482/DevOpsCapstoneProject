# LogiTrack API Testing Guide

## API Endpoints

### Inventory Controller

#### GET /api/inventory
- **Description**: Retrieve all inventory items
- **Response**: Array of InventoryItem objects
- **Example URL**: `GET http://localhost:5257/api/inventory`

#### GET /api/inventory/{id}
- **Description**: Retrieve specific inventory item by ID
- **Response**: Single InventoryItem object
- **Example URL**: `GET http://localhost:5257/api/inventory/1`

#### POST /api/inventory
- **Description**: Create a new inventory item
- **Request Body**:
```json
{
  "name": "Conveyor Belt",
  "quantity": 5,
  "location": "Warehouse C"
}
```

#### PUT /api/inventory/{id}
- **Description**: Update existing inventory item
- **Request Body**: Complete InventoryItem object with ID
```json
{
  "itemId": 1,
  "name": "Updated Pallet Jack",
  "quantity": 15,
  "location": "Warehouse A"
}
```

#### DELETE /api/inventory/{id}
- **Description**: Delete inventory item by ID
- **Example URL**: `DELETE http://localhost:5257/api/inventory/1`

### Order Controller

#### GET /api/order
- **Description**: Retrieve all orders with full item details
- **Response**: Array of OrderResponseDto objects

#### GET /api/order/{id}
- **Description**: Retrieve specific order by ID with items
- **Response**: Single OrderResponseDto object
- **Example URL**: `GET http://localhost:5257/api/order/1`

#### POST /api/order
- **Description**: Create a new order with items
- **Request Body**:
```json
{
  "customerName": "John Doe",
  "datePlaced": "2025-11-06T18:00:00Z",
  "items": [
    {
      "inventoryItemId": 1,
      "quantityOrdered": 2
    },
    {
      "inventoryItemId": 2,
      "quantityOrdered": 1
    }
  ]
}
```

#### PUT /api/order/{id}
- **Description**: Update existing order
- **Request Body**: CreateOrderDto object (same as POST)

#### DELETE /api/order/{id}
- **Description**: Delete order by ID
- **Example URL**: `DELETE http://localhost:5257/api/order/1`

## Testing Scenarios

### Scenario 1: Basic CRUD Operations

1. **Get all inventory items**
   ```
   GET /api/inventory
   Expected: Returns seeded inventory items
   ```

2. **Create new inventory item**
   ```
   POST /api/inventory
   Body: {"name": "Test Item", "quantity": 10, "location": "Test Location"}
   Expected: 201 Created with item details
   ```

3. **Create order with items**
   ```
   POST /api/order
   Body: Order with valid inventory item IDs
   Expected: 201 Created with order details
   ```

4. **Get order details**
   ```
   GET /api/order/{id}
   Expected: Order with full item details
   ```

### Scenario 2: Error Handling

1. **Get non-existent inventory item**
   ```
   GET /api/inventory/999
   Expected: 404 Not Found with error details
   ```

2. **Create order with invalid item ID**
   ```
   POST /api/order
   Body: Order with non-existent inventory item ID
   Expected: 400 Bad Request with validation error
   ```

3. **Create inventory item with empty name**
   ```
   POST /api/inventory
   Body: {"name": "", "quantity": 5, "location": "Test"}
   Expected: 400 Bad Request with validation error
   ```

### Scenario 3: Validation Testing

1. **Test quantity validation**
   ```
   POST /api/order
   Body: Order with quantity <= 0
   Expected: 400 Bad Request
   ```

2. **Test customer name validation**
   ```
   POST /api/order
   Body: Order with empty customer name
   Expected: 400 Bad Request
   ```

## Swagger UI Testing

Access Swagger UI at: `http://localhost:5257/swagger`

The Swagger interface provides:
- Interactive API documentation
- Built-in request/response testing
- Schema validation
- Example payloads

## Performance Features

1. **Async Operations**: All controller methods use async/await
2. **Efficient Queries**: Uses EF Core Include() for related data
3. **Proper Indexing**: Inventory items indexed on name and location
4. **Custom Error Handling**: Centralized error handling middleware
5. **DTO Mapping**: Separate DTOs for API contracts vs internal models

## Security Features

1. **Input Validation**: Data annotation validation on DTOs
2. **SQL Injection Protection**: EF Core parameterized queries
3. **Error Information**: Sanitized error responses
4. **Model State Validation**: Automatic validation of incoming requests

## Database Operations Verified

- ✅ Create, Read, Update, Delete for Inventory Items
- ✅ Create, Read, Update, Delete for Orders
- ✅ Many-to-Many relationship handling (Order-Items)
- ✅ Foreign key constraint validation
- ✅ Transaction integrity for multi-entity operations