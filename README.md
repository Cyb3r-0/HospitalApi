# 🏥 HospitalApi

A production-ready **ASP.NET Core 8** REST API for hospital management, featuring JWT authentication, role-based access control, Redis caching, Kafka event streaming, concurrency control, and rate limiting.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Database | SQL Server + Entity Framework Core 8 |
| Caching | Redis (StackExchange.Redis) |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net (work factor 12) |
| Message Queue | Apache Kafka (Confluent) |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 12 |
| Logging | Serilog (Console + File) |
| Docs | Swagger / OpenAPI |
| Containers | Docker + Docker Compose |

---

## 📁 Project Structure

```
HospitalApi/
├── Controllers/        # API endpoints
├── Services/           # Business logic layer
├── Repositories/       # Data access layer
├── Models/             # EF Core entity models
├── Dtos/               # Request / response DTOs
├── Validators/         # FluentValidation rules
├── Mapping/            # AutoMapper profiles
├── Middlewares/        # Global exception handler
├── Kafka/              # Producer + Consumer
├── Events/             # Kafka event models
├── Helpers/            # ApiResponse, PagedResult, PasswordHasher
├── Data/               # AppDbContext
└── Migrations/         # EF Core migrations

HospitalApi.Tools/
└── seeder.html         # Enterprise test suite (15 scenarios)
```

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- SQL Server (LocalDB or full)

### 1. Clone the repo

```bash
git clone https://github.com/your-username/HospitalApi.git
cd HospitalApi
```

### 2. Start infrastructure

```bash
docker compose up -d
```

Starts Redis, ZooKeeper, Kafka, and Kafka UI automatically.

### 3. Configure `appsettings.json`

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
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

> ⚠️ Never commit real secrets. Use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables in production.

### 4. Apply migrations and run

```bash
dotnet ef database update
dotnet run
```

Swagger UI: `https://localhost:{port}/swagger`  
Kafka UI: `http://localhost:8080`

---

## 🔐 Authentication & Roles

JWT Bearer tokens with BCrypt password hashing (work factor 12).

| Role | Permissions |
|---|---|
| `SuperAdmin` | Full access — register users, delete records |
| `Admin` | Read & write all modules |
| `Doctor` | Read & write patients and appointments |

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

---

## 📋 API Endpoints

### Auth
| Method | Endpoint | Rate Limit | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | 5/min | Login and get JWT token |
| `POST` | `/api/auth/register` | — | Register user (SuperAdmin only) |

### Patients
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/patients` | Any auth | Get all (paginated, filter by disease) |
| `GET` | `/api/patients/{id}` | Any auth | Get by ID |
| `POST` | `/api/patients` | Any auth | Create |
| `PUT` | `/api/patients/{id}` | Any auth | Update |
| `DELETE` | `/api/patients/{id}` | SuperAdmin | Delete |

### Doctors
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/doctors` | Any auth | Get all (filter by specialization, availability) |
| `GET` | `/api/doctors/{id}` | Any auth | Get by ID |
| `POST` | `/api/doctors` | SuperAdmin, Admin | Create |
| `PUT` | `/api/doctors/{id}` | SuperAdmin, Admin | Update |
| `DELETE` | `/api/doctors/{id}` | SuperAdmin | Delete |

### Appointments
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/appointments` | Any auth | Get all (filter by doctor, patient, status, date) |
| `GET` | `/api/appointments/{id}` | Any auth | Get by ID |
| `POST` | `/api/appointments` | Any auth | Book appointment |
| `PUT` | `/api/appointments/{id}` | Any auth | Update status |
| `DELETE` | `/api/appointments/{id}` | SuperAdmin | Soft delete |

### Bills
| Method | Endpoint | Role | Rate Limit | Description |
|---|---|---|---|---|
| `GET` | `/api/bills` | Any auth | — | Get all (filter by patient, doctor, status) |
| `GET` | `/api/bills/{id}` | Any auth | — | Get by ID with balance due |
| `POST` | `/api/bills` | SuperAdmin, Admin | — | Create bill (auto-calculates total) |
| `PUT` | `/api/bills/{id}` | SuperAdmin, Admin | — | Update fees/notes |
| `POST` | `/api/bills/{id}/pay` | SuperAdmin, Admin | 10/min | Record payment (supports partial) |
| `DELETE` | `/api/bills/{id}` | SuperAdmin | — | Soft delete |

### Payment Events (Kafka Stream)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/payment-events` | Full event stream (filter by bill, patient, doctor) |
| `GET` | `/api/payment-events/bill/{id}` | Complete payment history for a bill |
| `GET` | `/api/payment-events/patient/{id}` | All payments by a specific patient |

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

---

## ⚡ Caching

Redis distributed cache with graceful fallback — if Redis is unavailable the API continues serving from the database without crashing.

| Cache Key | TTL | Invalidated On |
|---|---|---|
| `patient_{id}` | 5 min | Update or Delete |
| `doctor_{id}` | 5 min | Update or Delete |
| `appointment_{id}` | 5 min | Update or Delete |
| `bill_{id}` | 5 min | Update, Pay, Delete |
| List queries | 2 min | Automatic expiry |

---

## 🔒 Concurrency Control

Bills use EF Core `[Timestamp]` RowVersion for optimistic concurrency. Two simultaneous payment requests on the same bill results in `409 Conflict` for the second request, preventing double payment.

---

## 🚦 Rate Limiting

ASP.NET Core built-in rate limiter — no extra packages needed.

| Policy | Limit | Applied To |
|---|---|---|
| `global` | 100 req/min per IP | All endpoints |
| `login` | 5 req/min per IP | `POST /api/auth/login` |
| `payment` | 10 req/min per IP | `POST /api/bills/{id}/pay` |

Exceeding limits returns `429 Too Many Requests`.

---

## 📨 Kafka Event Streaming

Every payment publishes a `PaymentMade` event to the `payment-events` topic. A background `IHostedService` consumer saves events to `PaymentEvents` table — giving you a complete, immutable audit trail even for partial/installment payments.

```
POST /api/bills/{id}/pay
  → Saves to DB
  → Publishes to Kafka
  → Consumer persists event
  → GET /api/payment-events/bill/{id} shows full history
```

Duplicate events are rejected by `EventId` (GUID) idempotency check.  
View events live: `http://localhost:8080`

---

## 🛡️ Error Handling

Global `ExceptionMiddleware` returns consistent JSON for all errors. Stack trace included in `DEBUG` builds only.

---

## 🗺️ Roadmap

- [x] JWT Authentication + BCrypt hashing
- [x] Role-based access control
- [x] Patient, Doctor, Appointment, Billing modules
- [x] Redis distributed caching with fallback
- [x] Kafka payment event streaming
- [x] Concurrency control (RowVersion / optimistic locking)
- [x] Rate limiting (global + per-endpoint)
- [x] Soft deletes with audit trail
- [x] FluentValidation + AutoMapper
- [x] Serilog structured logging
- [x] Docker + Docker Compose with persistent volumes
- [ ] API versioning
- [ ] Unit & integration tests
- [ ] CI/CD with GitHub Actions
- [ ] Cloud deployment

---

## 🧪 Testing

Open `HospitalApi.Tools/seeder.html` in Chrome. Enter credentials and click **Run** to execute 15 scenarios: auth, all CRUD modules, 4 payment modes, concurrency race conditions, Kafka validation, and data integrity checks.

---

## 📄 License

[MIT](LICENSE)