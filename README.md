# University Admissions System

A comprehensive microservices-based university admissions system built with .NET 8, featuring authentication, admission management, and automated notifications.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technologies Used](#technologies-used)
- [System Components](#system-components)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Running with Docker](#running-with-docker)
- [API Documentation](#api-documentation)
- [Development](#development)
- [CI/CD Pipeline](#cicd-pipeline)
- [Project Structure](#project-structure)

## 🏗️ Architecture Overview

This system follows a microservices architecture pattern with the following key components:

```
┌─────────────┐
│   Clients   │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│  API Gateway    │ (Ocelot - Port 5000)
│  (Port 5000)    │
└────────┬────────┘
         │
    ┌────┴─────────────────────────┐
    │                               │
    ▼                               ▼
┌──────────────┐          ┌─────────────────┐
│ Identity     │          │ Admission       │
│ Service      │          │ Service         │
│ (Port 5001)  │          │ (Port 5002)     │
└──────────────┘          └─────────────────┘
                                   │
                                   ▼
                          ┌─────────────────┐
                          │ Notification    │
                          │ Service         │
                          │ (Background)    │
                          └─────────────────┘
                                   │
                                   ▼
                          ┌─────────────────┐
                          │   PostgreSQL    │
                          │   (Port 5432)   │
                          └─────────────────┘
```

## 🛠️ Technologies Used

### Backend
- **.NET 8.0** - Latest LTS version
- **ASP.NET Core Web API** - RESTful API services
- **Entity Framework Core 8** - ORM for database operations
- **PostgreSQL 15** - Primary database
- **MediatR** - CQRS pattern implementation
- **Ocelot** - API Gateway
- **ASP.NET Core Identity** - Authentication and authorization
- **JWT Bearer Authentication** - Token-based security
- **MailKit** - Email sending for notifications

### Infrastructure
- **Docker & Docker Compose** - Containerization
- **GitHub Actions** - CI/CD pipeline

## 🎯 System Components

### 1. IdentityService (Port 5001)
**Purpose:** Handles user authentication and authorization

**Features:**
- User registration (Applicants, Faculty Managers, Head Managers)
- JWT token generation
- Role-based access control
- ASP.NET Core Identity integration

**Endpoints:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Authenticate and get JWT token

### 2. AdmissionService (Port 5002)
**Purpose:** Manages admission applications and education programs

**Features:**
- CQRS pattern with MediatR
- Applicant management
- Admission application lifecycle
- Education program management
- Document management (Passport, Education Documents)

**Key Endpoints:**
- `POST /api/applicants` - Create applicant profile
- `GET /api/applicants/{id}` - Get applicant details
- `POST /api/admissions` - Submit admission application
- `PUT /api/admissions/{id}/status` - Update admission status (Managers only)
- `GET /api/admissions/applicant/{applicantId}` - Get applicant's admissions

### 3. NotificationService (Background Worker)
**Purpose:** Sends email notifications to users

**Features:**
- Background worker service
- Automatic retry mechanism (up to 3 attempts)
- Email queue processing
- Error logging and tracking

**Notification Triggers:**
- Application submission
- Status changes (Under Review, Accepted, Rejected, Closed)
- System announcements

### 4. ApiGateway (Port 5000)
**Purpose:** Single entry point for all client requests

**Features:**
- Request routing with Ocelot
- JWT authentication enforcement
- CORS configuration
- Load balancing support

## 📊 Database Schema

### Core Entities

**Applicant**
- Personal information (Name, Email, BirthDate, Gender, Citizenship, PhoneNumber)
- Linked to multiple admissions and documents

**ApplicantAdmission**
- Links Applicant, Manager, and EducationProgram
- Status: Created → Under Review → Accepted/Rejected → Closed
- Timestamps for tracking

**Manager**
- Faculty managers and head managers
- Can be assigned to specific faculty or all faculties

**EducationProgram**
- Program details (Name, Code, Language, Form)
- Linked to Faculty and EducationLevel

**Document (Abstract)**
- Base class for all documents
- **Passport**: Series/Number, Place of Birth, Issued Date/By
- **EducationDocument**: Name, Type (High School, Bachelor, Master)

**Notification**
- Message queue for email sending
- Tracks sent status and retry attempts

### Dictionary Tables
- **EducationLevel**: Bachelor, Master, PhD
- **EducationDocumentType**: High School Diploma, Bachelor Diploma, Master Diploma
- **Faculty**: Computer Science, Engineering, Business Administration, etc.

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL 15](https://www.postgresql.org/download/) (if running locally without Docker)
- [Git](https://git-scm.com/)

### Clone the Repository

```bash
git clone <repository-url>
cd university-admissions
```

### Local Development Setup

1. **Restore NuGet packages:**
```bash
dotnet restore
```

2. **Build the solution:**
```bash
dotnet build
```

3. **Update database connection strings** in `appsettings.json` for each service if running locally

4. **Run database migrations:**
```bash
# IdentityService
cd src/IdentityService
dotnet ef database update

# AdmissionService
cd ../AdmissionService
dotnet ef database update
```

5. **Run services individually:**
```bash
# Terminal 1 - IdentityService
cd src/IdentityService
dotnet run

# Terminal 2 - AdmissionService
cd src/AdmissionService
dotnet run

# Terminal 3 - NotificationService
cd src/NotificationService
dotnet run

# Terminal 4 - ApiGateway
cd src/ApiGateway
dotnet run
```

## 🐳 Running with Docker

### Quick Start

1. **Navigate to docker directory:**
```bash
cd docker
```

2. **Start all services:**
```bash
docker-compose up --build
```

3. **Access the services:**
- API Gateway: http://localhost:5000
- IdentityService: http://localhost:5001
- AdmissionService: http://localhost:5002
- PostgreSQL: localhost:5432

4. **Stop all services:**
```bash
docker-compose down
```

### Environment Configuration

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Update the following in `.env`:
- Email SMTP settings (Gmail, SendGrid, etc.)
- JWT secret key (for production)
- Database credentials (if different)

## 📖 API Documentation

### Authentication Flow

1. **Register a new user:**
```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "applicant@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe",
  "role": 0  # 0=Applicant, 1=FacultyManager, 2=HeadManager
}
```

2. **Login and get JWT token:**
```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "applicant@example.com",
  "password": "SecurePass123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": "guid",
  "email": "applicant@example.com",
  "fullName": "John Doe",
  "role": "Applicant",
  "expiresAt": "2024-11-11T12:00:00Z"
}
```

3. **Use token in subsequent requests:**
```bash
GET http://localhost:5000/api/applicants/{id}
Authorization: Bearer {token}
```

### Admission Workflow

1. Create applicant profile
2. Submit admission application
3. Manager reviews and updates status
4. System sends notification to applicant
5. Applicant receives email updates

### Status Flow

```
Created → Under Review → Accepted/Rejected → Closed
```

## 💻 Development

### Adding a New Feature

1. **For AdmissionService (CQRS):**
   - Create command/query in `Features` folder
   - Create handler implementing `IRequestHandler`
   - Register in MediatR (automatic)
   - Add controller endpoint

2. **For Database Changes:**
   - Update entity in `Entities` folder
   - Update `DbContext` configuration
   - Create migration:
     ```bash
     dotnet ef migrations add MigrationName
     dotnet ef database update
     ```

### Code Structure (CQRS)

```
AdmissionService/
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
├── Entities/
├── Data/
└── Controllers/
```

## 🔄 CI/CD Pipeline

The project includes a GitHub Actions workflow that:

1. **Build & Test:** Compiles solution and runs unit tests
2. **Docker Build:** Creates Docker images for all services
3. **Database Migration:** Validates SQL scripts
4. **Deploy:** Placeholder for deployment automation

### Setup GitHub Actions

1. Add secrets to your repository:
   - `DOCKER_USERNAME`
   - `DOCKER_PASSWORD`

2. Push to `main` branch to trigger pipeline

## 📁 Project Structure

```
university-admissions/
├── src/
│   ├── Shared.Contracts/          # DTOs, Events, Enums
│   ├── IdentityService/           # Authentication & Authorization
│   ├── AdmissionService/          # Core business logic
│   ├── NotificationService/       # Email notifications
│   └── ApiGateway/                # Ocelot gateway
├── infra/
│   └── init-db-scripts/           # Database initialization
│       ├── 01-create-database.sql
│       ├── 02-create-tables.sql
│       ├── 03-seed-data.sql
│       ├── 04-constraints-indexes.sql
│       └── 05-seed-admin.sql
├── docker/
│   ├── docker-compose.yml
│   └── .env.example
├── .github/
│   └── workflows/
│       └── ci.yml                 # CI/CD pipeline
├── UniversityAdmissions.sln
└── README.md
```

## 🔐 Security Considerations

- All passwords are hashed using ASP.NET Core Identity
- JWT tokens expire after 24 hours
- Role-based authorization enforced at API level
- API Gateway validates tokens before routing
- CORS configured for specific origins (update in production)

## 📧 Email Configuration

For Gmail:
1. Enable 2-factor authentication
2. Generate app password
3. Update `appsettings.json` or environment variables:
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📝 License

This project is created for educational purposes.

## 👥 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📞 Support

For issues and questions, please open an issue in the GitHub repository.

---

**Built with ❤️ using .NET 8 and Microservices Architecture**
