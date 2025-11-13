# Solution Structure - University Admissions System

**Version:** 1.0  
**Platform:** .NET 8.0  
**Last Updated:** November 13, 2025

---

## Table of Contents

1. [Solution Overview](#solution-overview)
2. [Project Types](#project-types)
3. [Project Dependencies](#project-dependencies)
4. [Build Order](#build-order)
5. [NuGet Packages](#nuget-packages)
6. [Configuration Files](#configuration-files)

---

## Solution Overview

The solution consists of **5 projects** organized in a microservices architecture:

```
UniversityAdmissions.sln
├── src/
│   ├── Shared.Contracts/          [Class Library]
│   ├── IdentityService/           [Executable - Web API]
│   ├── AdmissionService/          [Executable - Web API]
│   ├── NotificationService/       [Executable - Worker Service]
│   └── ApiGateway/                [Executable - Web API]
```

---

## Project Types

### 1. Shared.Contracts (Class Library)

**Type:** Class Library  
**Target Framework:** net8.0  
**Output Type:** DLL

**Purpose:**
- Contains shared DTOs, enums, and event contracts
- Referenced by all services for consistency
- Enables loose coupling between services

**Key Components:**
- `DTOs/` - Data Transfer Objects
- `Enums/` - Shared enumerations (UserRole, AdmissionStatus, ManagerType, Gender)
- `Events/` - MassTransit event contracts for message bus

**References:**
- No project references (base library)

**NuGet Packages:**
- None (pure POCO classes)

---

### 2. IdentityService (Executable - Web API)

**Type:** ASP.NET Core Web API  
**Target Framework:** net8.0  
**Output Type:** Executable (EXE)  
**Port:** 5001

**Purpose:**
- User authentication and authorization
- JWT token generation and validation
- ASP.NET Core Identity integration
- User registration and login

**Project Structure:**
```
IdentityService/
├── Controllers/
│   └── AuthController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── DTOs/
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── RegisterRequest.cs
├── Models/
│   └── ApplicationUser.cs
├── Services/
│   ├── AuthService.cs
│   └── TokenService.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── nlog.config
└── Program.cs
```

**Project References:**
- `Shared.Contracts` (for enums and DTOs)

**Key NuGet Packages:**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.x)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.x)
- `Microsoft.EntityFrameworkCore.Design` (8.0.x)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.x)
- `AutoMapper` (15.1.0)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
- `NLog.Web.AspNetCore` (6.1.0)

**Database:**
- PostgreSQL (Identity Database)
- Connection String: `Host=postgres;Port=5432;Database=identity_service;...`

---

### 3. AdmissionService (Executable - Web API)

**Type:** ASP.NET Core Web API  
**Target Framework:** net8.0  
**Output Type:** Executable (EXE)  
**Port:** 5002

**Purpose:**
- Core business logic for admissions
- CQRS pattern implementation with MediatR
- Background jobs with Quartz.NET
- Event publishing with MassTransit/RabbitMQ
- External API integration

**Project Structure:**
```
AdmissionService/
├── Controllers/
│   ├── AdmissionsController.cs
│   ├── ApplicantsController.cs
│   ├── DictionaryController.cs
│   └── HealthController.cs
├── Data/
│   └── AdmissionDbContext.cs
├── Entities/
│   ├── Applicant.cs
│   ├── ApplicantAdmission.cs
│   ├── Manager.cs
│   ├── Faculty.cs
│   ├── EducationProgram.cs
│   ├── EducationLevel.cs
│   ├── Document.cs (abstract)
│   ├── Passport.cs
│   ├── EducationDocument.cs
│   ├── EducationDocumentType.cs
│   ├── AdmissionProgram.cs
│   ├── File.cs
│   └── Notification.cs
├── Features/
│   ├── Applicants/
│   │   ├── Commands/
│   │   │   ├── CreateApplicantCommand.cs
│   │   │   └── CreateApplicantHandler.cs
│   │   └── Queries/
│   │       ├── GetApplicantByIdQuery.cs
│   │       └── GetApplicantByIdHandler.cs
│   └── Admissions/
│       ├── Commands/
│       └── Queries/
├── Jobs/
│   ├── CleanupAdmissionsJob.cs
│   └── ExternalDataSyncJob.cs
├── Services/
│   ├── IExternalDictionaryService.cs
│   └── ExternalDictionaryService.cs
├── DTOs/
│   └── ExternalApiDtos.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── nlog.config
└── Program.cs
```

**Project References:**
- `Shared.Contracts` (for enums and events)

**Key NuGet Packages:**
- `Microsoft.EntityFrameworkCore.Design` (8.0.x)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.x)
- `MediatR` (12.4.x)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.x)
- `Quartz` (3.9.0)
- `Quartz.Extensions.Hosting` (3.9.0)
- `Quartz.Extensions.DependencyInjection` (3.9.0)
- `MassTransit` (8.x)
- `MassTransit.RabbitMQ` (8.x)
- `AutoMapper` (15.1.0)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
- `NLog.Web.AspNetCore` (6.1.0)

**Database:**
- PostgreSQL (Admission Database)
- Connection String: `Host=postgres;Port=5432;Database=admission_service;...`

**Background Jobs (Quartz):**
1. **CleanupAdmissionsJob** - Runs daily at 3 AM
   - Closes applications under review for >3 days
   - Cron: `0 0 3 * * ?` (configurable)

2. **ExternalDataSyncJob** - Runs every 6 hours
   - Syncs Faculty and EducationProgram data from external API
   - Cron: `0 0 */6 * * ?` (configurable)

---

### 4. NotificationService (Executable - Worker Service)

**Type:** Worker Service (Background Service)  
**Target Framework:** net8.0  
**Output Type:** Executable (EXE)  
**Port:** N/A (background worker)

**Purpose:**
- Consume events from RabbitMQ
- Send email notifications
- Process notification queue with retry logic
- Background worker for continuous processing

**Project Structure:**
```
NotificationService/
├── Consumers/
│   └── ApplicantStatusChangedConsumer.cs
├── Controllers/
│   └── NotificationController.cs
├── Data/
│   └── NotificationDbContext.cs
├── Entities/
│   └── Notification.cs
├── Migrations/
│   └── [EF Core Migrations]
├── Services/
│   ├── IEmailService.cs
│   └── EmailService.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── nlog.config
├── Worker.cs
└── Program.cs
```

**Project References:**
- `Shared.Contracts` (for event contracts)

**Key NuGet Packages:**
- `Microsoft.EntityFrameworkCore.Design` (8.0.x)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.x)
- `MassTransit` (8.x)
- `MassTransit.RabbitMQ` (8.x)
- `MailKit` (4.3.x) - Email sending
- `Microsoft.Extensions.Hosting` (8.0.x)
- `AutoMapper` (15.1.0)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
- `NLog.Web.AspNetCore` (6.1.0)

**Database:**
- PostgreSQL (Notification Database)
- Connection String: `Host=postgres;Port=5432;Database=notification_service;...`

**Background Processing:**
- **Worker.cs** - Runs continuously
  - Polls database for queued notifications
  - Sends emails via SMTP (Gmail/SendGrid)
  - Updates notification status
  - Retry logic (up to 3 attempts)

**RabbitMQ Consumers:**
- `ApplicantStatusChangedConsumer` - Listens for admission status changes

---

### 5. ApiGateway (Executable - Web API)

**Type:** ASP.NET Core Web API with Ocelot  
**Target Framework:** net8.0  
**Output Type:** Executable (EXE)  
**Port:** 5000

**Purpose:**
- Central entry point for all client requests
- Route requests to appropriate microservices
- JWT authentication enforcement
- CORS configuration
- Load balancing (future)

**Project Structure:**
```
ApiGateway/
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── ocelot.json
└── Program.cs
```

**Project References:**
- None (routes to services via HTTP)

**Key NuGet Packages:**
- `Ocelot` (23.3.x)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.x)

**Routing Configuration (ocelot.json):**
```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "identity-service", "Port": 80 }
      ]
    },
    {
      "UpstreamPathTemplate": "/api/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "admission-service", "Port": 80 }
      ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ]
}
```

---

## Project Dependencies

### Dependency Graph

```
┌──────────────────────┐
│  Shared.Contracts    │ (No dependencies)
└──────────┬───────────┘
           │
    ┌──────┴──────┬──────────────┬──────────────┐
    │             │              │              │
    ▼             ▼              ▼              ▼
┌─────────┐ ┌──────────┐ ┌──────────────┐ ┌──────────┐
│Identity │ │Admission │ │Notification  │ │ApiGateway│
│Service  │ │Service   │ │Service       │ │(HTTP)    │
└─────────┘ └──────────┘ └──────────────┘ └──────────┘
```

**Direct Dependencies:**
- **IdentityService** → Shared.Contracts
- **AdmissionService** → Shared.Contracts
- **NotificationService** → Shared.Contracts
- **ApiGateway** → None (HTTP routing)

**Service Communication:**
- Services communicate via **RabbitMQ** (event-driven)
- Clients communicate via **ApiGateway** (REST API)
- No direct service-to-service HTTP calls

---

## Build Order

The solution must be built in this order to resolve dependencies:

1. **Shared.Contracts** (no dependencies)
2. **IdentityService** (depends on Shared.Contracts)
3. **AdmissionService** (depends on Shared.Contracts)
4. **NotificationService** (depends on Shared.Contracts)
5. **ApiGateway** (no project dependencies)

### Build Command
```bash
dotnet build UniversityAdmissions.sln --configuration Release
```

### Build Output
```
src/
├── Shared.Contracts/bin/Release/net8.0/Shared.Contracts.dll
├── IdentityService/bin/Release/net8.0/IdentityService.exe
├── AdmissionService/bin/Release/net8.0/AdmissionService.exe
├── NotificationService/bin/Release/net8.0/NotificationService.exe
└── ApiGateway/bin/Release/net8.0/ApiGateway.exe
```

---

## NuGet Packages

### Common Packages (All Services)

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.App` | 8.0.x | ASP.NET Core framework |
| `Microsoft.NET.Sdk.Web` | 8.0.x | Web SDK |
| `AutoMapper` | 15.1.0 | Object mapping (MANDATORY) |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 | DI integration |
| `NLog.Web.AspNetCore` | 6.1.0 | Structured logging (MANDATORY) |

### Database Packages

| Package | Version | Services |
|---------|---------|----------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.x | Identity, Admission, Notification |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.x | Identity, Admission, Notification |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.x | Identity only |

### Messaging Packages

| Package | Version | Services |
|---------|---------|----------|
| `MassTransit` | 8.x | Admission, Notification |
| `MassTransit.RabbitMQ` | 8.x | Admission, Notification |

### Background Jobs

| Package | Version | Services |
|---------|---------|----------|
| `Quartz` | 3.9.0 | Admission (MANDATORY) |
| `Quartz.Extensions.Hosting` | 3.9.0 | Admission |
| `Quartz.Extensions.DependencyInjection` | 3.9.0 | Admission |

### Other Packages

| Package | Version | Services | Purpose |
|---------|---------|----------|---------|
| `MediatR` | 12.4.x | Admission | CQRS pattern |
| `MailKit` | 4.3.x | Notification | Email sending |
| `Ocelot` | 23.3.x | ApiGateway | API Gateway |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.x | All | JWT auth |

---

## Configuration Files

### appsettings.json Structure

Each service has its own configuration:

**IdentityService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=identity_service;..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "UniversityAdmissionsIdentityService",
    "Audience": "UniversityAdmissionsClients"
  }
}
```

**AdmissionService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=admission_service;..."
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Quartz": {
    "CleanupCron": "0 0 3 * * ?",
    "SyncCron": "0 0 */6 * * ?"
  },
  "ExternalApi": {
    "BaseUrl": "https://1c-mockup.kreosoft.space",
    "Username": "student",
    "Password": "..."
  }
}
```

**NotificationService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=notification_service;..."
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "...",
    "Password": "...",
    "FromName": "University Admissions System"
  }
}
```

**ApiGateway:**
```json
{
  "Jwt": {
    "Key": "...",
    "Issuer": "UniversityAdmissionsIdentityService",
    "Audience": "UniversityAdmissionsClients"
  }
}
```

### NLog Configuration (nlog.config)

Each service has its own `nlog.config` for structured logging:
- File logging to `c:\temp\nlog-{ServiceName}-*.log`
- Console logging for Docker
- Log level filtering (Info and above)

---

## Deployment Considerations

### Docker Deployment

Each service has a `Dockerfile` (to be created):
- Multi-stage build
- Uses `mcr.microsoft.com/dotnet/aspnet:8.0` runtime
- Exposes appropriate ports
- Environment variable configuration

### Environment Variables

Override `appsettings.json` via environment variables:
```bash
ConnectionStrings__DefaultConnection=...
Jwt__Key=...
RabbitMQ__Host=...
```

### Database Migrations

Apply migrations before starting services:
```bash
# IdentityService
dotnet ef database update --project src/IdentityService

# AdmissionService
dotnet ef database update --project src/AdmissionService

# NotificationService
dotnet ef database update --project src/NotificationService
```

Or use init scripts in `infra/init-db-scripts/`

---

## Testing

### Unit Tests (Future)

Recommended structure:
```
tests/
├── IdentityService.Tests/
├── AdmissionService.Tests/
├── NotificationService.Tests/
└── Integration.Tests/
```

### Integration Tests

Test service communication:
- RabbitMQ event publishing/consuming
- Database operations
- External API calls

---

## CI/CD

### GitHub Actions Workflow

`.github/workflows/ci.yml`:
1. Restore dependencies
2. Build solution
3. Run tests
4. Build Docker images
5. Push to registry (optional)

---

## Project Metadata

### Target Framework
- All projects: **.NET 8.0** (LTS)

### Language Version
- C# 12

### Nullable Reference Types
- Enabled in all projects

### ImplicitUsings
- Enabled for cleaner code

---

## Summary

### Project Count: 5
- **Class Libraries:** 1 (Shared.Contracts)
- **Executables:** 4 (IdentityService, AdmissionService, NotificationService, ApiGateway)

### Total Dependencies
- **Mandatory Libraries:** AutoMapper ✅, Quartz ✅, NLog ✅
- **Total NuGet Packages:** ~25 unique packages
- **Database:** PostgreSQL (3 separate databases)
- **Message Broker:** RabbitMQ

### Lines of Code (Approximate)
- **Total:** ~5000+ lines
- **Shared.Contracts:** ~200 lines
- **IdentityService:** ~800 lines
- **AdmissionService:** ~2500 lines
- **NotificationService:** ~600 lines
- **ApiGateway:** ~100 lines

---

**Documentation Version:** 1.0  
**Last Updated:** November 13, 2025
