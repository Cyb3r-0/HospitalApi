# 🏥 HospitalApi

A RESTful Web API for hospital management built with **ASP.NET Core 8** and **Entity Framework Core**, featuring JWT-based authentication, role-based authorization, AutoMapper, and a billing system with concurrency control.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Domain Models](#domain-models)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Authentication](#authentication)
- [Roadmap](#roadmap)

---

## Overview

HospitalApi provides a backend system to manage patients, doctors, appointments, billing, and user authentication for a hospital environment. It is designed for extensibility and follows RESTful conventions with clean separation of concerns.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Authentication | JWT Bearer Tokens |
| Object Mapping | AutoMapper |
| Testing | xUnit |
| Version Control | Git (GitHub) |

---

## Project Structure

```
HospitalApi/
├── Controllers/          # API endpoint controllers
│   ├── AuthController.cs
│   ├── PatientController.cs
│   ├── DoctorController.cs
│   ├── AppointmentController.cs
│   └── BillController.cs
├── Data/
│   └── AppDbContext.cs   # EF Core DbContext
├── Models/               # Domain entities
│   ├── Patient.cs
│   ├── Doctor.cs
│   ├── Appointment.cs
│   ├── Bill.cs
│   ├── User.cs
│   └── Role.cs
├── Dtos/                 # Data Transfer Objects
├── Mapping/              # AutoMapper profiles
│   └── DoctorProfile.cs
├── Migrations/           # EF Core migrations
├── appsettings.json
└── Program.cs
```

---

## Features

- **Patient Management** — Create, read, update, and delete patient records
- **Doctor Management** — Manage doctor profiles and availability
- **Appointment Scheduling** — Book and track appointments between patients and doctors
- **Billing System** — Generate and manage bills with concurrency conflict detection
- **JWT Authentication** — Secure token-based login and authorization
- **Role-Based Access Control** — Admin and staff role enforcement across all endpoints
- **Audit Trail** — Track which user created/updated each record
- **AutoMapper** — Clean DTO-to-entity mapping

---

## Domain Models

### Patient
Stores patient information and links to the user who registered them.

### Doctor
Stores doctor profiles including specialty and the user who registered them.

### Appointment
Links a patient to a doctor with a scheduled date and status (`Scheduled`, `Completed`, `Cancelled`).

### Bill
Financial record linked to an appointment. Includes concurrency control via a `RowVersion` field to prevent conflicting updates.

### User & Role
Users authenticate via JWT. Each user is assigned a role (`Admin`, `Staff`, etc.) that controls access to protected endpoints.

---

## API Endpoints

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Login and receive JWT token |
| POST | `/api/auth/register` | Register a new user |

### Patients
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/patients` | Get all patients |
| GET | `/api/patients/{id}` | Get patient by ID |
| POST | `/api/patients` | Create new patient |
| PUT | `/api/patients/{id}` | Update patient |
| DELETE | `/api/patients/{id}` | Delete patient |

### Doctors
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/doctors` | Get all doctors |
| GET | `/api/doctors/{id}` | Get doctor by ID |
| POST | `/api/doctors` | Create new doctor |
| PUT | `/api/doctors/{id}` | Update doctor |
| DELETE | `/api/doctors/{id}` | Delete doctor |

### Appointments
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/appointments` | Get all appointments |
| GET | `/api/appointments/{id}` | Get appointment by ID |
| POST | `/api/appointments` | Book an appointment |
| PUT | `/api/appointments/{id}` | Update appointment |
| DELETE | `/api/appointments/{id}` | Cancel appointment |

### Bills
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/bills` | Get all bills |
| GET | `/api/bills/{id}` | Get bill by ID |
| POST | `/api/bills` | Create bill |
| PUT | `/api/bills/{id}` | Update bill (with concurrency check) |
| DELETE | `/api/bills/{id}` | Delete bill |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Azure)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Cyb3r-0/HospitalApi.git
   cd HospitalApi
   ```

2. **Configure the database**

   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=HospitalDb;Trusted_Connection=True;"
     }
   }
   ```

3. **Configure JWT**
   ```json
   {
     "Jwt": {
       "Key": "your-secret-key-here",
       "Issuer": "HospitalApi",
       "Audience": "HospitalApiUsers"
     }
   }
   ```

4. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

---

## Authentication

All protected endpoints require a **Bearer token** in the `Authorization` header:

```
Authorization: Bearer <your-jwt-token>
```

To obtain a token, call `POST /api/auth/login` with valid credentials. The token must be included on all subsequent requests to protected routes.

---

## Roadmap

The following improvements are planned for future releases:

- **Enterprise architecture** — Separation into class library layers (Repository, Service, Domain, Infrastructure, API)
- **Stored Procedures** — Replace direct `AppDbContext` calls with SQL Server stored procedures for better performance and security
- **Unit of Work pattern** — Transactional consistency across repositories
- **Cloud Deployment** — Azure App Service + Azure SQL Database
- **Swagger / OpenAPI** — Full API documentation
- **Pagination & filtering** — For list endpoints
- **Logging** — Structured logging with Serilog

---

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

---

## License

This project is open source and available under the [MIT License](LICENSE).