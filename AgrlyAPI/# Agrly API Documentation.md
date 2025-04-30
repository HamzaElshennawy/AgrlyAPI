# Agrly API Documentation

This is the documentation for the Agrly API, a .NET 9.0 Web API project using JWT authentication and Supabase as the database.

## Table of Contents

- [Getting Started](#getting-started)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
- [Development Setup](#development-setup)
- [Database Schema](#database-schema)

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Supabase account and project

### Configuration

Update `appsettings.json` with your credentials:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "JwtConfig": {
    "Issuer": "https://localhost:7202",
    "Audience": "https://localhost:7202",
    "Key": "your_jwt_secret_key",
    "TokenValidityMins": 30
  },
  "AllowedHosts": "*",
  "SupabaseUrl": "your_supabase_url",
  "SupabaseKey": "your_supabase_key"
}
```

```http
POST /api/AuthenticateUser/login
Content-Type: application/json

Request Body:
{
  "username": "your_username",
  "password": "your_password"
}

Response:
{
  "username": "your_username",
  "token": "eyJhbGciOiJIUzUxMiI...",
  "expiresIn": 30
}
```

# Authentication

**For protected endpoints, include the JWT token in the request header:**

```http

Authorization: Bearer your_jwt_token
```

## API Endpoints

### **User Management**

#### _Create User_

```http

POST /api/users/adduserAuthorization: Bearer your_jwt_tokenContent-Type: application/jsonRequest
Body:
{
    "username": "newuser",
    "firstName": "John",
    "lastName": "Doe",
    "email": "<john@example.com>",
    "password": "secure_password"
}

Response:
{
    "id": 123
}

```

### Get All Users

```http

GET /api/users/getallusers
Authorization: Bearer your_jwt_token
Response:[
    {
        "id": 123,
        "username": "user1",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com",
        "createdAt": "2024-01-28T12:00:00Z"
    }
]
```

### Delete User

```http

DELETE /api/users/deleteuser/{id}
Authorization: Bearer your_jwt_token
Response: 204 No Content
Error Handling
The API uses standard HTTP status codes:

Status Code Description
200 Success
400 Bad Request - Invalid input
401 Unauthorized - Invalid or missing token
403 Forbidden - Insufficient permissions
404 Not Found - Resource doesn't exist
409 Conflict - Resource already exists
Error Response Format
json

{ "message": "Error description"}
```

## Development Setup

- Clone the repository
- Configure appsettings.json
- Run the application:

```bash
Run
dotnet watch
```

### Access points

Development: <https://localhost:7202>

Swagger UI: <https://localhost:7202/swagger>

## Database Schema

### Users Table

```sql

CREATE TABLE users
    (
        id BIGINT PRIMARY KEY,
        username TEXT UNIQUE NOT NULL,
        first_name TEXT,
        last_name TEXT,
        email TEXT,
        password TEXT NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
```

## Security Features

### JWT Authentication

- 30-minute token expiration
- Secure token validation
- HTTPS enforcement
- Password Security
- Password hashing using secure algorithms
- Input validation
- Protection against common vulnerabilities
- Authorization

Role-based access control (prepared for implementation)
User-specific operations validation
Project Structure

```plaintext
AgrlyAPI/
├── Controllers/
│ └── Users/
│ ├── AuthenticateUser.cs
│ └── usersController.cs
├── Models/
│ ├── Api/
│ │ ├── LoginRequestModel.cs
│ │ └── LoginResponseModel.cs
│ └── User/
│ └── User.cs
├── Services/
│ ├── JwtService.cs
│ └── PasswordHashHandler.cs
└── Program.cs
```

### License

This project is licensed under the MIT License.
