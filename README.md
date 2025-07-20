# Orders API

A clean, maintainable Orders API built with C#, .NET 8 Web API, and SQLite database, demonstrating modern backend architecture and best practices.




### POST /api/orders
Creates a new order with items.

**Request Body:**
```json
{
  "customerName": "John Doe",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "quantity": 2
    },
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "quantity": 1
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customerName": "John Doe",
  "createdAt": "2024-01-20T12:34:56.789Z",
  "items": [
    {
      "id": 1,
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "quantity": 2
    },
    {
      "id": 2,
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "quantity": 1
    }
  ]
}
```

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQLite (included in .NET)

### Installation
1.  Restore NuGet packages
```bash
dotnet restore
```

2. Apply database migrations
```bash
dotnet ef database update
```

3. Run the application
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5240`
- HTTPS: `https://localhost:7240`

##  Data Seeding

The application uses SQLite database with Entity Framework Core migrations. The database is automatically created when you run:

```bash
dotnet ef database update
```



### Test Coverage
The project includes comprehensive unit tests with **NUnit** and **Moq**:

- **Controllers Tests**: API endpoint behavior testing
- **Services Tests**: Business logic validation

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```



### Swagger UI
Access the interactive API documentation at:
```
http://localhost:5240/swagger/index.html
```



