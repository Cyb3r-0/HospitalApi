# 🏥 HospitalApi

A production-ready **ASP.NET Core 8** REST API for hospital management, featuring JWT authentication, role-based access control, Redis caching, and a clean layered architecture.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Database | SQL Server + Entity Framework Core 8 |
| Caching | Redis (StackExchange.Redis) |
| Authentication | JWT Bearer Tokens |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 12 |
| Logging | Serilog (Console + File) |
| Docs | Swagger / OpenAPI |

---

## 📁 Project Structure

```
HospitalApi/
├── Controllers/        # API endpoints (Auth, Patients)
├── Services/           # Business logic layer
├── Repositories/       # Data access layer
├── Models/             # EF Core entity models
├── Dtos/               # Request / response data transfer objects
├── Validators/         # FluentValidation rules
├── Mapping/            # AutoMapper profiles
├── Middlewares/        # Global exception handler
├── Helpers/            # ApiResponse, PagedResult, PasswordHasher
├── Data/               # AppDbContext
└── Migrations/         # EF Core migrations
```

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full)
- Redis (local or [Docker](#running-redis-with-docker))

### 1. Clone the repo

```bash
git clone https://github.com/your-username/HospitalApi.git
cd HospitalApi
```

### 2. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HospitalDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS_LONG",
    "Issuer": "HospitalApi",
    "Audience": "HospitalApiUsers",
    "DurationInMinutes": 60
  }
}
```

> ⚠️ Never commit real secrets. Use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables in production.

### 3. Apply database migrations

```bash
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run
```

Swagger UI will be available at: `https://localhost:{port}/swagger`

---

### Running Redis with Docker

If you don't have Redis installed locally, spin it up with Docker:

```bash
docker run -d -p 6379:6379 --name redis redis:alpine
```

---

## 🔐 Authentication & Roles

The API uses **JWT Bearer tokens**. Three roles are supported:

| Role | Permissions |
|---|---|
| `SuperAdmin` | Full access — including register users & delete patients |
| `Admin` | Read & write patients |
| `Doctor` | Read & write patients |

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "yourpassword"
}
```

Returns a JWT token. Pass it in all subsequent requests:

```http
Authorization: Bearer <your_token>
```

### Register a new user *(SuperAdmin only)*

```http
POST /api/auth/register
Authorization: Bearer <superadmin_token>

{
  "username": "dr.smith",
  "password": "SecurePass123",
  "roleId": 3
}
```

---

## 📋 API Endpoints

### Patients

| Method | Endpoint | Role Required | Description |
|---|---|---|---|
| `GET` | `/api/patients` | Any auth | Get all patients (paginated) |
| `GET` | `/api/patients/{id}` | Any auth | Get patient by ID |
| `POST` | `/api/patients` | Any auth | Create a patient |
| `PUT` | `/api/patients/{id}` | Any auth | Update a patient |
| `DELETE` | `/api/patients/{id}` | SuperAdmin | Delete a patient |

### Query Parameters for `GET /api/patients`

| Param | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 50) |
| `disease` | string | — | Filter by disease name |

**Example:**
```http
GET /api/patients?page=1&pageSize=10&disease=diabetes
```

---

## 📦 Response Format

All endpoints return a consistent wrapper:

```json
{
  "success": true,
  "message": "Patients fetched successfully",
  "data": { ... }
}
```

Paginated responses include:

```json
{
  "success": true,
  "message": "Patients fetched successfully",
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

---

## ⚡ Caching Strategy

Redis is used to cache frequently read data:

| Cache Key | TTL | Invalidated On |
|---|---|---|
| `patient_{id}` | 5 minutes | Update or Delete |
| `patients_page{n}_size{n}_disease_{x}` | 2 minutes | Automatic expiry |

---

## 📝 Logging

Serilog writes structured logs to:
- **Console** — for local development
- **`Logs/log-{date}.txt`** — rolling daily log file

Log level is configured in `appsettings.json` under the `Serilog` section.

---

## 🛡️ Error Handling

A global `ExceptionMiddleware` catches all unhandled exceptions and returns a consistent JSON error response:

```json
{
  "statusCode": 404,
  "message": "Resource not found"
}
```

Exception-to-status-code mapping:

| Exception | Status Code |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| Any other | 500 Internal Server Error |

In `DEBUG` builds, the stack trace is also included in the response.

---

## 🗺️ Roadmap

- [x] JWT Authentication + RBAC
- [x] Patient CRUD with pagination
- [x] Redis distributed caching
- [x] FluentValidation + AutoMapper
- [x] Serilog structured logging
- [x] Global exception middleware
- [x] Doctors & Appointments modules
- [x] Billing module
- [x] BCrypt password hashing
- [x] Rate limiting
- [ ] API versioning
- [x] Unit & integration tests
- [x] Docker + Docker Compose
- [ ] CI/CD with GitHub Actions

---

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

---

## 📄 License

[MIT](LICENSE)
