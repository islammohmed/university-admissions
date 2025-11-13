# System Architecture - University Admissions System

**Version:** 1.0  
**Platform:** .NET 8.0  
**Architecture Pattern:** Microservices  
**Last Updated:** November 13, 2025

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Architecture Principles](#architecture-principles)
4. [High-Level Architecture](#high-level-architecture)
5. [Service Architecture](#service-architecture)
6. [Data Architecture](#data-architecture)
7. [Communication Patterns](#communication-patterns)
8. [Security Architecture](#security-architecture)
9. [Deployment Architecture](#deployment-architecture)
10. [Design Patterns](#design-patterns)
11. [Technology Stack](#technology-stack)
12. [Scalability & Performance](#scalability--performance)
13. [Monitoring & Logging](#monitoring--logging)
14. [Future Enhancements](#future-enhancements)

---

## Executive Summary

The University Admissions System is a modern, cloud-ready microservices application built on .NET 8.0. It manages the complete admission lifecycle from application submission to final acceptance/rejection, with automated notifications and background processing.

### Key Features
- ✅ **Microservices Architecture** - 4 independent, scalable services
- ✅ **Event-Driven Communication** - RabbitMQ message broker
- ✅ **CQRS Pattern** - Optimized read/write operations
- ✅ **Background Jobs** - Automated data sync and email notifications
- ✅ **API Gateway** - Centralized routing with Ocelot
- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Role-Based Authorization** - Applicant, Manager, HeadManager, Admin
- ✅ **PostgreSQL Database** - Reliable relational database
- ✅ **Docker Support** - Containerized deployment
- ✅ **Health Checks** - Built-in monitoring endpoints

---

## System Overview

### Business Context

The system facilitates university admissions by:
1. Allowing applicants to create profiles and submit applications
2. Enabling managers to review and process applications
3. Automatically notifying stakeholders of status changes
4. Synchronizing reference data from external systems
5. Managing education programs, faculties, and document types

### System Boundaries

**In Scope:**
- User authentication and authorization
- Applicant profile management
- Admission application workflow
- Document management (metadata)
- Email notifications
- Dictionary data management
- Background job processing

**Out of Scope:**
- Payment processing
- Document storage (files)
- Student information system
- Course registration
- Academic transcript management

---

## Architecture Principles

### 1. Microservices
Each service is:
- **Independently deployable**
- **Loosely coupled**
- **Highly cohesive**
- **Technology agnostic** (can use different tech stacks)
- **Failure isolated** (one service failure doesn't crash others)

### 2. Database Per Service
Each service has its own database:
- **Identity Database** - User accounts
- **Admission Database** - Business logic
- **Notification Database** - Email queue

### 3. API-First Design
All services expose RESTful APIs following standard conventions

### 4. Event-Driven Communication
Services communicate via events (RabbitMQ) for loose coupling

### 5. Scalability
Services can scale independently based on load

### 6. Resilience
Built-in retry logic, health checks, and graceful degradation

---

## High-Level Architecture

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Applications                      │
│              (Web Browser, Mobile App, etc.)                 │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTPS
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   API Gateway (Ocelot)                       │
│                       Port 5000                              │
│  - JWT Authentication Enforcement                            │
│  - Request Routing                                           │
│  - CORS Configuration                                        │
│  - Load Balancing (future)                                   │
└───────┬──────────────────────────┬──────────────────────────┘
        │                          │
        │ REST API                 │ REST API
        │                          │
        ▼                          ▼
┌──────────────────┐      ┌──────────────────────────┐
│ Identity Service │      │   Admission Service       │
│    Port 5001     │      │       Port 5002           │
│                  │      │                           │
│ - User Auth      │      │ - Business Logic          │
│ - JWT Tokens     │      │ - CQRS (MediatR)          │
│ - Registration   │      │ - Quartz Jobs             │
│ - Login          │      │ - Event Publishing        │
└────────┬─────────┘      └────────┬──────────────────┘
         │                         │
         │                         │ Events (RabbitMQ)
         ▼                         ▼
   ┌──────────┐            ┌─────────────┐
   │PostgreSQL│            │  RabbitMQ   │
   │Identity  │            │  Message    │
   │   DB     │            │  Broker     │
   └──────────┘            └──────┬──────┘
         │                        │
         │                        │ Consume Events
         │                        ▼
         │              ┌──────────────────────┐
         │              │ Notification Service │
         │              │   (Worker Service)   │
         │              │                      │
         │              │ - RabbitMQ Consumer  │
         │              │ - Email Sending      │
         │              │ - Background Worker  │
         │              └──────────┬───────────┘
         │                         │
         ▼                         ▼
   ┌──────────┐            ┌──────────────┐
   │PostgreSQL│            │  PostgreSQL  │
   │Admission │            │ Notification │
   │   DB     │            │     DB       │
   └──────────┘            └──────────────┘
         │
         │ HTTP (Scheduled)
         ▼
   ┌──────────────┐
   │ External API │
   │ (Dictionary) │
   │    Data      │
   └──────────────┘
```

### Component Interaction Flow

**1. User Registration Flow:**
```
Client → API Gateway → Identity Service → PostgreSQL (Identity DB)
                                       ↓
                                   JWT Token
                                       ↓
                                   Client
```

**2. Create Admission Flow:**
```
Client → API Gateway → Admission Service → PostgreSQL (Admission DB)
                              ↓
                          Publish Event
                              ↓
                          RabbitMQ
                              ↓
                    Notification Service
                              ↓
                         Send Email
```

**3. Background Sync Flow:**
```
Quartz Scheduler → External API → Parse Data → Update Database
                                                     ↓
                                              Log Results
```

---

## Service Architecture

### 1. Identity Service

**Responsibilities:**
- User authentication and authorization
- JWT token generation and validation
- User registration (Applicants, Managers, Admins)
- Password management
- Role assignment

**Technology:**
- ASP.NET Core 8.0 Web API
- ASP.NET Core Identity
- JWT Bearer authentication
- PostgreSQL with Entity Framework Core

**API Endpoints:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Authenticate and get token

**Database Tables:**
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- etc. (Identity schema)

**Security:**
- Password hashing (ASP.NET Core Identity)
- Token expiration (24 hours)
- Secure password requirements

---

### 2. Admission Service

**Responsibilities:**
- Core business logic
- Applicant profile management
- Admission application lifecycle
- Education program management
- Document management (metadata)
- Background job processing
- Event publishing

**Technology:**
- ASP.NET Core 8.0 Web API
- MediatR (CQRS pattern)
- Quartz.NET (background jobs)
- MassTransit + RabbitMQ (events)
- PostgreSQL with Entity Framework Core

**Architecture Pattern:**
```
Controllers → MediatR → Handlers → DbContext
                  ↓
             Events → RabbitMQ
```

**API Endpoints:**
- `POST /api/applicants` - Create applicant
- `GET /api/applicants/{id}` - Get applicant
- `POST /api/admissions` - Create admission
- `GET /api/admissions/{id}` - Get admission
- `PUT /api/admissions/{id}/status` - Update status (Manager)
- `GET /api/admissions/applicant/{id}` - Get applicant's admissions
- `GET /api/dictionary/*` - Get dictionary data

**Database Tables:**
- Applicant
- ApplicantAdmission
- Manager
- Faculty
- EducationProgram
- EducationLevel
- Document (abstract)
- Passport
- EducationDocument
- EducationDocumentType
- AdmissionProgram
- File
- Notification (tracking)

**Background Jobs:**

1. **CleanupAdmissionsJob**
   - Frequency: Daily at 3:00 AM
   - Purpose: Auto-close applications under review for >3 days
   - Configuration: `Quartz:CleanupCron` in appsettings.json
   - Default: `0 0 3 * * ?`

2. **ExternalDataSyncJob**
   - Frequency: Every 6 hours (configurable)
   - Purpose: Sync Faculty and EducationProgram data
   - Configuration: `Quartz:SyncCron` in appsettings.json
   - Default: `0 0 */6 * * ?`
   - External API: `https://1c-mockup.kreosoft.space`

**Events Published:**
- `ApplicantStatusChangedEvent` - When admission status changes
- `ApplicantRegisteredEvent` - When new applicant registers

---

### 3. Notification Service

**Responsibilities:**
- Consume events from RabbitMQ
- Send email notifications
- Process notification queue
- Retry failed notifications

**Technology:**
- Worker Service (.NET 8.0)
- MassTransit + RabbitMQ (consumer)
- MailKit (email sending)
- PostgreSQL with Entity Framework Core

**Architecture Pattern:**
```
RabbitMQ Consumer → Handler → EmailService → SMTP
                         ↓
                    Save to DB
```

**Components:**

1. **Background Worker**
   - Runs continuously
   - Polls database for queued notifications
   - Sends emails
   - Updates notification status
   - Retry logic (up to 3 attempts)

2. **RabbitMQ Consumers**
   - `ApplicantStatusChangedConsumer`
   - Listens for status change events
   - Creates notification records

**Email Configuration:**
- SMTP Server: Gmail, SendGrid, or custom
- Port: 587 (TLS)
- Authentication: Username/password or API key

**Database Tables:**
- Notification (Id, Message, UserEmail, IsSent, SentAt, RetryCount, ErrorMessage)

---

### 4. API Gateway

**Responsibilities:**
- Central entry point
- Request routing
- JWT authentication enforcement
- CORS configuration
- Load balancing (future)

**Technology:**
- ASP.NET Core 8.0
- Ocelot Gateway

**Routing Configuration:**
```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamHostAndPorts": [
        { "Host": "identity-service", "Port": 80 }
      ]
    },
    {
      "UpstreamPathTemplate": "/api/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
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

## Data Architecture

### Database Design

#### Entity-Relationship Diagram

See `docs/database-schema.drawio` for visual representation.

**Key Relationships:**

1. **Applicant ↔ ApplicantAdmission** (1:N)
   - One applicant can have multiple admissions

2. **Applicant ↔ Document** (1:N)
   - One applicant can upload multiple documents

3. **Document ↔ File** (N:1)
   - Multiple documents can reference same file

4. **Document Inheritance** (TPH)
   - Passport extends Document
   - EducationDocument extends Document

5. **Manager ↔ ApplicantAdmission** (1:N)
   - One manager reviews multiple admissions

6. **Manager ↔ Faculty** (N:1, optional)
   - FacultyManager belongs to one faculty
   - HeadManager has no faculty (null)

7. **Faculty ↔ EducationProgram** (1:N)
   - One faculty offers multiple programs

8. **EducationProgram ↔ EducationLevel** (N:1)
   - Many programs belong to one level

9. **ApplicantAdmission ↔ AdmissionProgram** (1:N)
   - One admission can select multiple programs with priority

10. **EducationDocumentType ↔ EducationLevel** (Two relationships)
    - **BelongsTo** (N:1): Document type belongs to a level
    - **NextAvailableLevels** (N:M): Document type allows admission to levels

### Database Normalization

- **3NF (Third Normal Form)** achieved
- No transitive dependencies
- Proper foreign key constraints
- Indexes on frequently queried columns

### Data Seeding

**Automated Seeding:**
1. Education Levels (Bachelor, Master, PhD)
2. Faculties (Computer Science, Engineering, Business)
3. Education Programs
4. Document Types
5. Admin and Manager users

**Seeding Scripts:**
- `infra/init-db-scripts/03-seed-data.sql`
- `IdentityService/Data/DbInitializer.cs`

---

## Communication Patterns

### Synchronous Communication (REST API)

**Client → API Gateway → Services**

- Used for request/response scenarios
- JWT authentication on each request
- Timeout: 30 seconds
- Retry: Client-side (not implemented)

### Asynchronous Communication (Events)

**Service → RabbitMQ → Service**

- Used for loosely coupled interactions
- Fire-and-forget pattern
- Guaranteed delivery (RabbitMQ persistence)
- Automatic retry on consumer failure

**Event Flow Example:**
```
Admission Status Updated
     ↓
Publish "ApplicantStatusChangedEvent"
     ↓
RabbitMQ Exchange
     ↓
NotificationService Consumer
     ↓
Create Notification Record
     ↓
Send Email
```

### External Integration

**Admission Service → External API**

- HTTP GET requests
- Basic authentication
- Scheduled via Quartz.NET
- Error handling and logging

---

## Security Architecture

### Authentication

**JWT (JSON Web Tokens)**
- Issued by Identity Service
- Lifetime: 24 hours
- Contains: UserId, Email, Role
- Signed with HS256 algorithm

### Authorization

**Role-Based Access Control (RBAC)**

| Role | Permissions |
|------|-------------|
| **Applicant** | Create profile, Submit applications, View own data |
| **FacultyManager** | Review applications for specific faculty, Update status |
| **HeadManager** | Review all applications, Manage entire campaign |
| **Admin** | System administration, User management |

### API Security

1. **HTTPS Only** (production)
2. **JWT Validation** on protected endpoints
3. **Role Checks** via `[Authorize(Roles = "...")]`
4. **CORS** configured for specific origins

### Data Security

1. **Password Hashing** - ASP.NET Core Identity (PBKDF2)
2. **SQL Injection Prevention** - EF Core parameterized queries
3. **XSS Prevention** - ASP.NET Core built-in
4. **Sensitive Data** - Not logged or exposed in errors

---

## Deployment Architecture

### Docker Containers

Each service runs in its own container:

```
┌─────────────────────────────────────┐
│         Docker Host                 │
├─────────────────────────────────────┤
│  ┌─────────────┐  ┌──────────────┐ │
│  │ API Gateway │  │   Identity   │ │
│  │  (Port5000) │  │  (Port 5001) │ │
│  └─────────────┘  └──────────────┘ │
│  ┌─────────────┐  ┌──────────────┐ │
│  │  Admission  │  │Notification  │ │
│  │ (Port 5002) │  │  (Worker)    │ │
│  └─────────────┘  └──────────────┘ │
│  ┌─────────────┐  ┌──────────────┐ │
│  │ PostgreSQL  │  │  RabbitMQ    │ │
│  │ (Port 5432) │  │ (Port 5672)  │ │
│  └─────────────┘  └──────────────┘ │
└─────────────────────────────────────┘
```

### Docker Compose

**Services:**
- apigateway
- identity-service
- admission-service
- notification-service
- postgres
- rabbitmq

**Networks:**
- `university-network` (bridge)

**Volumes:**
- `postgres-data` - Database persistence
- `rabbitmq-data` - Message queue persistence

### Environment Variables

Configured via `.env` file:
- Database credentials
- JWT secret key
- SMTP settings
- External API credentials

---

## Design Patterns

### 1. Microservices Pattern
- Service per bounded context
- Independent deployment
- Decentralized data management

### 2. CQRS (Command Query Responsibility Segregation)
- Separate read and write operations
- Implemented via MediatR
- Optimized queries

### 3. Repository Pattern
- Abstraction over data access
- Implemented via EF Core DbContext
- Unit of Work pattern

### 4. Gateway Pattern
- API Gateway (Ocelot)
- Single entry point
- Request routing

### 5. Event-Driven Architecture
- Publish/Subscribe pattern
- RabbitMQ message broker
- Loose coupling

### 6. Background Worker Pattern
- Long-running tasks
- Scheduled jobs (Quartz)
- Continuous processing (Worker Service)

### 7. Retry Pattern
- Email sending retry (up to 3 attempts)
- Exponential backoff (future)

### 8. Health Check Pattern
- Liveness checks (`/health`)
- Readiness checks (`/health/ready`)
- Database connectivity verification

---

## Technology Stack

### Backend Framework
- **.NET 8.0** (LTS release)
- **ASP.NET Core** Web API
- **C# 12**

### Libraries (Mandatory)
- ✅ **AutoMapper 15.1.0** - Object mapping
- ✅ **Quartz.NET 3.9.0** - Job scheduling
- ✅ **NLog 6.0.6** - Structured logging

### Data Access
- **Entity Framework Core 8.0**
- **PostgreSQL 15** (Npgsql provider)
- **Code-First Migrations**

### Messaging
- **MassTransit 8.x** - Message bus abstraction
- **RabbitMQ** - Message broker

### API Gateway
- **Ocelot 23.3** - API Gateway

### Authentication
- **ASP.NET Core Identity**
- **JWT Bearer**
- **Microsoft.IdentityModel.Tokens**

### Patterns
- **MediatR 12.4** - CQRS implementation
- **FluentValidation** (future)

### Email
- **MailKit 4.3** - SMTP client

### Testing (Future)
- **xUnit**
- **Moq**
- **FluentAssertions**

### DevOps
- **Docker**
- **Docker Compose**
- **GitHub Actions**

---

## Scalability & Performance

### Horizontal Scaling

Each service can scale independently:

```bash
# Scale Admission Service to 3 instances
docker-compose up --scale admission-service=3

# API Gateway load balances automatically
```

### Vertical Scaling

- Increase container resources (CPU/Memory)
- Configure in `docker-compose.yml`

### Database Optimization

1. **Indexes** on foreign keys and frequently queried columns
2. **Connection Pooling** (EF Core default)
3. **Async/Await** for all database operations
4. **Pagination** for large result sets

### Caching (Future)

- **Redis** for distributed caching
- Cache dictionary data
- Cache frequently accessed applicant data

### Performance Targets

- API Response Time: <200ms (95th percentile)
- Background Job Processing: <1 minute
- Email Delivery: <30 seconds

---

## Monitoring & Logging

### Logging

**NLog Configuration:**
- Console logging (Docker)
- File logging (`c:\temp\nlog-*.log`)
- Structured logging
- Log levels: Trace, Debug, Info, Warn, Error, Fatal

**Log Correlation:**
- Request ID tracking (future)
- Distributed tracing (future)

### Health Checks

All services expose:
- `GET /health` - Basic liveness
- `GET /health/ready` - Readiness with DB check

**Monitoring Tools (Future):**
- Prometheus
- Grafana
- ELK Stack (Elasticsearch, Logstash, Kibana)

### Metrics (Future)

- Request count
- Response time
- Error rate
- Database connection pool usage
- RabbitMQ queue depth

---

## Future Enhancements

### Phase 2 Features

1. **File Storage**
   - Azure Blob Storage or AWS S3
   - Document upload/download
   - Virus scanning

2. **Two-Factor Authentication (2FA)**
   - TOTP (Time-based One-Time Password)
   - SMS verification
   - Email verification codes

3. **Real-Time Notifications**
   - SignalR for WebSocket connections
   - Push notifications
   - In-app notifications

4. **Advanced Search**
   - Elasticsearch integration
   - Full-text search
   - Fuzzy matching

5. **Reporting**
   - Export to PDF/Excel
   - Analytics dashboard
   - Admission statistics

6. **Workflow Engine**
   - Configurable approval workflows
   - Multi-step review process
   - Document verification workflow

### Technical Improvements

1. **API Versioning**
   - Support multiple API versions
   - Backward compatibility

2. **Rate Limiting**
   - Protect against abuse
   - Per-user quotas

3. **Distributed Tracing**
   - OpenTelemetry
   - Jaeger or Zipkin

4. **Circuit Breaker**
   - Polly library
   - Resilience policies

5. **CQRS with Event Sourcing**
   - Complete audit trail
   - Event store

6. **GraphQL**
   - Alternative to REST
   - Flexible queries

---

## Compliance & Standards

### Mandatory Requirements ✅

1. ✅ **2-3 Deployable Units** - 4 services implemented
2. ✅ **AutoMapper** - Installed in all services
3. ✅ **Quartz** - Background jobs implemented
4. ✅ **NLog** - Structured logging configured
5. ✅ **Background Task 1** - External data sync (daily)
6. ✅ **Background Task 2** - Email notifications (continuous)
7. ✅ **Health Checks** - All services have `/health` endpoints
8. ✅ **Data Seeding** - Admin and manager accounts seeded

### Bonus Features ✅

1. ✅ **RabbitMQ** - Event-driven notification service
2. ⏳ **Two-Factor Authentication** - Planned for Phase 2
3. ✅ **API Gateway** - Ocelot implementation

### Best Practices

- ✅ RESTful API design
- ✅ Separation of concerns
- ✅ Dependency injection
- ✅ Configuration management
- ✅ Error handling
- ✅ Documentation

---

## Conclusion

The University Admissions System is a production-ready, scalable microservices application built with modern .NET technologies. It follows industry best practices and architectural patterns to ensure maintainability, scalability, and reliability.

### Key Achievements

- **Fully functional microservices** with clear boundaries
- **Event-driven architecture** for loose coupling
- **Automated background processing** for data sync and notifications
- **Comprehensive security** with JWT and RBAC
- **Docker support** for easy deployment
- **Health checks** for monitoring
- **Extensive documentation** for developers

### Next Steps

1. Deploy to production environment (Azure/AWS)
2. Implement monitoring and alerting
3. Add automated testing (unit, integration, E2E)
4. Implement Phase 2 features
5. Performance tuning and optimization
6. Security audit and penetration testing

---

**Architecture Version:** 1.0  
**Last Updated:** November 13, 2025  
**Maintained By:** Development Team  

---

## References

- [README.md](../README.md) - Getting started guide
- [API-SPECIFICATION.md](./API-SPECIFICATION.md) - API documentation
- [SOLUTION-STRUCTURE.md](./SOLUTION-STRUCTURE.md) - Project structure
- [architecture.drawio](./architecture.drawio) - Architecture diagram
- [database-schema.drawio](./database-schema.drawio) - Database schema

---

**For questions or contributions, please open an issue or pull request in the repository.**
