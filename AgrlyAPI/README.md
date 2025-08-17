![Agrly Logo](assets/images/logo_orange.png)

# Agrly API Documentation

This is the documentation for the Agrly API, a .NET 9.0 Web API project using JWT authentication and Supabase as the database.

## Table of Contents

* [Getting Started](#getting-started)
* [Authentication](#authentication)
* [API Endpoints](#api-endpoints)
  * [User Management](#user-management)
  * [Transactions](#transactions)
  * [Media Assets](#media-assets)
  * [Apartments](#apartments)
  * [Reviews](#reviews)
* [Error Handling](#error-handling)
* [Development Setup](#development-setup)
* [Database Schema](#database-schema)
* [Security Features](#security-features)
* [Project Structure](#project-structure)
* [License](#license)

## Getting Started

### Prerequisites

* .NET 9.0 SDK
* Supabase account and project

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

## Authentication

Include the JWT token in the request header for protected endpoints:

```
Authorization: Bearer your_jwt_token
```

### Obtain a Token

**POST** `/api/AuthenticateUser/login`

Request Body:

```json
{
  "username": "your_username",
  "password": "your_password"
}
```

Response:

```json
{
  "username": "your_username",
  "token": "eyJhbGciOiJIUzUxMiI...",
  "expiresIn": 30
}
```

## API Endpoints

### User Management

#### Create User

**POST** `/api/users/adduser`

Request:

```json
{
  "username": "newuser",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "secure_password"
}
```

Response:

```json
{
  "id": 123
}
```

#### Get All Users

**GET** `/api/users/getallusers`

Response:

```json
[
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

#### Delete User

**DELETE** `/api/users/deleteuser/{id}`

Response: `204 No Content`

---

### Transactions

#### Get All Transactions (Admin Only)

**GET** `/api/transactions`

Response:

```json
[
  {
    "id": 1,
    "senderID": 1,
    "receiverID": 2,
    "amount": 100.0,
    "currency": "USD",
    "status": "pending",
    "method": "bank_transfer",
    "billingID": 10,
    "createdAt": "2024-01-28T12:00:00Z"
  }
]
```

#### Get Transaction by ID (Sender/Receiver Only)

**GET** `/api/transactions/{id}`

Response:

```json
{
  "id": 1,
  "senderID": 1,
  "receiverID": 2,
  "amount": 100.0,
  "currency": "USD",
  "status": "pending",
  "method": "bank_transfer",
  "billingID": 10,
  "createdAt": "2024-01-28T12:00:00Z"
}
```

#### Create a Transaction

**POST** `/api/transactions`

Request:

```json
{
  "senderID": 1,
  "receiverID": 2,
  "amount": 100.0,
  "currency": "USD",
  "status": "pending",
  "method": "bank_transfer",
  "billingID": 10
}
```

Response:

```json
{
  "id": 1,
  "senderID": 1,
  "receiverID": 2,
  "amount": 100.0,
  "currency": "USD",
  "status": "pending",
  "method": "bank_transfer",
  "billingID": 10,
  "createdAt": "2024-01-28T12:00:00Z"
}
```

---

### Media Assets

#### Get All Media for User's Apartments

**GET** `/api/MediaAssets/apartments`

Response:

```json
[
  {
    "id": 1,
    "userID": 1,
    "apartmetnID": 2,
    "filePath": "user-1/abc123_photo.jpg",
    "publicUrl": "https://your-supabase-url/storage/v1/object/public/user-media/user-1/abc123_photo.jpg",
    "type": "apartment_photo",
    "uploadedAt": "2024-01-28T12:00:00Z"
  }
]
```

#### Upload Media

**POST** `/api/MediaAssets/upload`

Content-Type: multipart/form-data

Response:

```json
{
  "url": "https://your-supabase-url/storage/v1/object/public/user-media/user-1/filename.jpg"
}
```

---

### Apartments

#### Get All Apartments

**GET** `/api/apartments`

#### Get Available Apartments (Paginated)

**GET** `/api/apartments/getavailable?currentPage={page}`

#### Get Apartment by ID (with Reviews)

**GET** `/api/apartments/{id}`

#### Get Owned Apartments

**GET** `/api/apartments/owned`

#### Create Apartment

**POST** `/api/apartments`

#### Update Apartment

**PUT** `/api/apartments/{id}`

#### Delete Apartment

**DELETE** `/api/apartments/{id}`

#### Search Apartments

**GET** `/api/apartments/search?query={query}&currentPage={page}`

#### Get Apartments by Location

**GET** `/api/apartments/get_property_by_location/{location}`

#### Get Apartments by Category

**POST** `/api/apartments/get_property_by_gategory/`

#### Get Categories

**GET** `/api/apartments/categories`

#### Add Category

**POST** `/api/apartments/categories/add`

#### Update Category

**PUT** `/api/apartments/categories/update`

#### Get Rent History

**GET** `/api/apartments/gethistory/{userid}`

#### Book Apartment

**POST** `/api/apartments/{apartmentId}/book`

#### Add Review to Apartment

**POST** `/api/apartments/{apartmentId}/review`

---

### Reviews

#### Reviews Model

The `Reviews` model supports apartment reviews with multiple rating fields, comments, and host responses.

| Field                | Type      | Description                  |
|----------------------|-----------|------------------------------|
| id                   | bigint    | Primary key                  |
| booking_id           | bigint    | Related booking              |
| reviewer_id          | bigint    | User who wrote the review    |
| apartment_id         | bigint    | Apartment being reviewed     |
| host_id              | bigint    | Host user                    |
| overall_rating       | int       | Overall rating               |
| cleanliness_rating   | int       | Cleanliness rating           |
| communication_rating | int       | Communication rating         |
| check_in_rating      | int       | Check-in rating              |
| accuracy_rating      | int       | Accuracy rating              |
| location_rating      | int       | Location rating              |
| value_rating         | int       | Value rating                 |
| title                | string    | Review title                 |
| comment              | string    | Review comment               |
| review_type          | string    | Type of review               |
| host_response        | string    | Host's response              |
| host_response_date   | datetime  | Date of host response        |
| created_at           | datetime  | Created timestamp            |
| updated_at           | datetime  | Updated timestamp            |

#### Get Reviews for Apartment

**GET** `/api/apartments/{id}`

Returns apartment details and associated reviews.

#### Add Review to Apartment

**POST** `/api/apartments/{apartmentId}/review`

Request:

```json
{
  "bookingId": 1,
  "reviewerId": 2,
  "apartmentId": 3,
  "hostId": 4,
  "overallRating": 5,
  "cleanlinessRating": 5,
  "communicationRating": 5,
  "checkInRating": 5,
  "accuracyRating": 5,
  "locationRating": 5,
  "valueRating": 5,
  "title": "Great stay!",
  "comment": "Everything was perfect.",
  "reviewType": "guest",
  "hostResponse": null,
  "hostResponseDate": null
}
```

---

## Error Handling

| Status Code | Description                        |
| ----------- | ---------------------------------- |
| 200         | Success                            |
| 400         | Bad Request - Invalid input        |
| 401         | Unauthorized - Invalid/missing JWT |
| 403         | Forbidden - Insufficient rights    |
| 404         | Not Found - Resource doesn't exist |
| 409         | Conflict - Resource already exists |

Error format:

```json
{ "message": "Error description" }
```

---

## Development Setup

* Clone the repository
* Configure `appsettings.json`
* Run the application:

```bash
dotnet watch
```

### Access Points

* Development: [https://localhost:7202](https://localhost:7202)
* Swagger UI: [https://localhost:7202/swagger](https://localhost:7202/swagger)
* Scalar UI: [http://localhost:5258/scalar](http://localhost:5258/scalar)

---

## Database Schema

### Users Table

```sql
CREATE TABLE users (
  id BIGINT PRIMARY KEY,
  username TEXT UNIQUE NOT NULL,
  first_name TEXT,
  last_name TEXT,
  email TEXT,
  password TEXT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Transactions Table

```sql
CREATE TABLE transactions (
  id BIGINT PRIMARY KEY,
  senderid BIGINT,
  receiverid BIGINT,
  amount FLOAT,
  currency TEXT,
  status TEXT,
  method TEXT,
  billingID BIGINT,
  createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Files Table

```sql
CREATE TABLE files (
  id BIGINT PRIMARY KEY,
  user_id BIGINT,
  apartment_id BIGINT,
  file_path TEXT,
  public_url TEXT,
  type TEXT,
  uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Reviews Table

```sql
CREATE TABLE reviews (
  id BIGINT PRIMARY KEY,
  booking_id BIGINT,
  reviewer_id BIGINT,
  apartment_id BIGINT,
  host_id BIGINT,
  overall_rating INTEGER,
  cleanliness_rating INTEGER,
  communication_rating INTEGER,
  check_in_rating INTEGER,
  accuracy_rating INTEGER,
  location_rating INTEGER,
  value_rating INTEGER,
  title VARCHAR,
  comment TEXT,
  review_type VARCHAR,
  host_response TEXT,
  host_response_date TIMESTAMPTZ,
  created_at TIMESTAMPTZ,
  updated_at TIMESTAMPTZ
);
```

---

## Security Features

* JWT authentication with 30-minute token expiration
* HTTPS enforcement
* Secure password hashing
* Input validation
* Protection against common vulnerabilities
* Role-based access control (admin/user)
* User-specific operation validation
* Rate limiting on sensitive endpoints

---

## Project Structure

```plaintext
AgrlyAPI/
├── Controllers/
│   ├── Media/
│   │   └── MediaAssets.cs
│   ├── Users/
│   │   ├── AuthenticateUser.cs
│   │   └── usersController.cs
│   ├── Transactions/
│   │   └── TransactionsController.cs
│   ├── Apartments/
│   │   └── ApartmentsController.cs
│   └── docs.cs
├── Models/
│   ├── Api/
│   │   ├── LoginRequestModel.cs
│   │   └── LoginResponseModel.cs
│   │   ├── ApartmentByIdResponse.cs
│   │   └── RentHistoryResponse.cs
│   ├── Apartments/
│   │   ├── Apartment.cs
│   │   ├── Booking.cs
│   │   ├── RentHistory.cs
│   │   ├── Reviews.cs
│   ├── User/
│   │   ├── User.cs
│   │   ├── Billing.cs
│   │   └── Transactions.cs
│   └── Users/
│       └── Photos.cs
├── Services/
│   ├── JwtService.cs
│   └── PasswordHashHandler.cs
├── Program.cs
└── AgrlyAPI.csproj
```

---

## TODO:
- Add Google Login and Signup
- Implement review creation endpoint logic
- Add more validation and error handling for booking and review endpoints

---
## License

This project is licensed under the MIT License.
