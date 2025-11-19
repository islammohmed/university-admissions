# 🎓 University Admissions System - Client Delivery Guide

## 📋 Project Overview

The University Admissions System is a modern, enterprise-grade web application built with **.NET 8.0** that manages the complete university admission process from application submission to acceptance/rejection, with automated email notifications.

### Key Features Delivered

✅ **Complete Admission Management System**
- Applicant registration and profile management
- Multiple admission applications per applicant
- Document tracking (Passport, Education Documents)
- Status workflow (Created → Under Review → Accepted/Rejected → Closed)
- Education program and faculty management

✅ **Multi-Role User System**
- **Applicants**: Submit applications and track status
- **Faculty Managers**: Review applications for specific faculty
- **Head Managers**: Oversee all admissions across university
- **Admins**: System administration

✅ **Automated Email Notifications**
- Application submission confirmations
- Status change notifications
- Background processing with automatic retry

✅ **External System Integration**
- Automatic synchronization with external API for faculties and programs
- Scheduled background jobs for data updates
- Automated cleanup of stale applications

✅ **Security & Authentication**
- JWT token-based authentication
- Role-based access control
- Secure password management
- API gateway for centralized security

---

## 🏗️ System Architecture

### Components

The system consists of **4 independent services** working together:

```
┌─────────────────────────────────────────────┐
│         Web/Mobile Applications             │
│         (Your Frontend - Not Included)      │
└─────────────────┬───────────────────────────┘
                  │ HTTPS
                  ▼
┌─────────────────────────────────────────────┐
│           API Gateway (Port 5000)           │
│         Single Entry Point for All APIs     │
└────────┬────────────────────────────────────┘
         │
    ┌────┴─────────────────────┐
    │                          │
    ▼                          ▼
┌──────────────┐      ┌────────────────────┐
│Identity      │      │ Admission Service  │
│Service       │      │   (Port 5002)      │
│(Port 5001)   │      │                    │
│              │      │ - Applications     │
│- Registration│      │ - Programs         │
│- Login       │      │ - Documents        │
│- JWT Tokens  │      │ - Background Jobs  │
└──────────────┘      └──────┬─────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │ Notification   │
                    │   Service      │
                    │  (Background)  │
                    │                │
                    │- Email Sending │
                    │- Retry Logic   │
                    └────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │  PostgreSQL    │
                    │  Database      │
                    │  (Port 5432)   │
                    └────────────────┘
```

### Service Descriptions

1. **API Gateway (Port 5000)**
   - Acts as single entry point for all client requests
   - Routes requests to appropriate services
   - Validates JWT tokens for security
   - Handles CORS for web applications

2. **Identity Service (Port 5001)**
   - User registration (Applicants, Managers, Admins)
   - Login and JWT token generation
   - Password management and security
   - User profile information

3. **Admission Service (Port 5002)**
   - Core business logic
   - Applicant profiles and applications
   - Education programs and faculties
   - Document management
   - Background jobs (data sync, cleanup)
   - Status workflow management

4. **Notification Service (Background Worker)**
   - Monitors database for pending email notifications
   - Sends emails via SMTP
   - Automatic retry (up to 3 attempts) for failed emails
   - Tracks delivery status

---

## 📊 Database Structure

The system uses **PostgreSQL 15** database with the following main entities:

### Core Tables

1. **Applicant** - Applicant personal information
   - FullName, Email, BirthDate, Gender, Citizenship, PhoneNumber

2. **ApplicantAdmission** - Admission applications
   - Status: Created → UnderReview → Accepted/Rejected → Closed
   - Links applicant to programs and managers

3. **Manager** - Faculty and Head managers
   - Can be assigned to specific faculty or oversee all

4. **Faculty** - Academic faculties
   - Computer Science, Engineering, Business, etc.

5. **EducationProgram** - Degree programs
   - Name, Code, Language (Russian/English)
   - Linked to Faculty and Level (Bachelor/Master/PhD)

6. **Document** - Applicant documents (with inheritance)
   - **Passport**: Series, Number, Issued Date, Place of Birth
   - **EducationDocument**: Type (High School/Bachelor/Master Diploma)

7. **Notification** - Email queue and history
   - Message, Recipient, Status, Retry Count

### Dictionary Tables
- **EducationLevel**: Bachelor, Master, PhD
- **EducationDocumentType**: High School Diploma, Bachelor Diploma, Master Diploma

---

## 🚀 How to Run the Project

### Prerequisites

Before running the project, ensure you have:

1. **Windows 10/11** (already installed)
2. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **PostgreSQL 15** - [Download here](https://www.postgresql.org/download/windows/)
4. **Visual Studio 2022** or **VS Code** (optional, for development)

### Step 1: Install PostgreSQL

1. Download and install PostgreSQL 15
2. During installation, set password for 'postgres' user (use: `1234` or update in configuration)
3. Keep default port: `5432`

### Step 2: Configure Database

1. Open **pgAdmin** or **psql** command line
2. Run the initialization scripts in order:

```sql
-- Navigate to: infra/init-db-scripts/
-- Run these files in order:
1. 01-create-database.sql      -- Creates 'university' database
2. 02-create-tables.sql         -- Creates all tables
3. 03-seed-data.sql             -- Adds sample data
4. 04-constraints-indexes.sql   -- Adds constraints and indexes
5. 05-seed-admin.sql            -- Creates admin users
```

**Using psql command:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\infra\init-db-scripts"

psql -U postgres -c "CREATE DATABASE university;"
psql -U postgres -d university -f 02-create-tables.sql
psql -U postgres -d university -f 03-seed-data.sql
psql -U postgres -d university -f 04-constraints-indexes.sql
psql -U postgres -d university -f 05-seed-admin.sql
```

### Step 3: Configure Email (for Notifications)

1. Open `src\NotificationService\appsettings.json`
2. Update email configuration:

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "FromName": "University Admissions",
    "FromAddress": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

**For Gmail:**
- Enable 2-factor authentication
- Generate app password: https://myaccount.google.com/apppasswords
- Use app password in configuration

### Step 4: Verify Database Connection

Check connection strings in all service `appsettings.json` files:

**IdentityService/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=university;Username=postgres;Password=1234"
  }
}
```

**AdmissionService/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=university;Username=postgres;Password=1234"
  }
}
```

**NotificationService/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=university;Username=postgres;Password=1234"
  }
}
```

### Step 5: Run the Services

You need to run **4 services** simultaneously. Open **4 separate PowerShell windows**:

**Window 1 - API Gateway:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\ApiGateway"
dotnet run
```
*Should start on: http://localhost:5000*

**Window 2 - Identity Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\IdentityService"
dotnet run
```
*Should start on: http://localhost:5001*

**Window 3 - Admission Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\AdmissionService"
dotnet run
```
*Should start on: http://localhost:5002*

**Window 4 - Notification Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\NotificationService"
dotnet run
```
*Runs as background worker (no HTTP endpoint)*

### Step 6: Verify Services are Running

Open browser and check:
- http://localhost:5000/health - API Gateway
- http://localhost:5001/health - Identity Service
- http://localhost:5002/health - Admission Service

All should return: `Healthy`

---

## 🧪 Testing the System

### Option 1: Using Postman (Recommended)

Import the provided Postman collection:
- File: `postman-collection.json` (in root folder)
- Import into Postman
- All endpoints are pre-configured with examples

### Option 2: Manual API Testing

#### 1. Register a New Applicant

**Request:**
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe",
  "role": 0
}
```

**Roles:**
- `0` = Applicant
- `1` = Faculty Manager
- `2` = Head Manager

**Response:**
```json
{
  "succeeded": true,
  "message": "User registered successfully"
}
```

#### 2. Login

**Request:**
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "guid-here",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Applicant",
  "expiresAt": "2024-11-17T12:00:00Z"
}
```

**⚠️ Important: Copy the `token` value - you'll need it for subsequent requests!**

#### 3. Create Applicant Profile

**Request:**
```http
POST http://localhost:5000/api/applicants
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "birthDate": "2000-01-15T00:00:00Z",
  "gender": 0,
  "citizenship": "USA",
  "phoneNumber": "+1234567890"
}
```

**Gender Values:**
- `0` = Male
- `1` = Female

**Response:**
```json
{
  "id": "applicant-guid",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "birthDate": "2000-01-15T00:00:00Z",
  "gender": "Male",
  "citizenship": "USA",
  "phoneNumber": "+1234567890"
}
```

#### 4. Submit Admission Application

**Request:**
```http
POST http://localhost:5000/api/admissions
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "applicantId": "applicant-guid-from-step-3",
  "educationProgramId": "e1111111-1111-1111-1111-111111111111"
}
```

**Pre-seeded Program IDs:**
- `e1111111-1111-1111-1111-111111111111` - Computer Science (Bachelor)
- `e2222222-2222-2222-2222-222222222222` - Software Engineering (Bachelor)
- `e3333333-3333-3333-3333-333333333333` - Data Science (Master)
- `e4444444-4444-4444-4444-444444444444` - Mechanical Engineering (Bachelor)
- `e5555555-5555-5555-5555-555555555555` - Business Administration (Bachelor)

**Response:**
```json
{
  "id": "admission-guid",
  "applicantId": "applicant-guid",
  "status": "Created",
  "createdAt": "2024-11-16T10:00:00Z"
}
```

**✉️ An email notification will be sent automatically!**

#### 5. Check Application Status

**Request:**
```http
GET http://localhost:5000/api/admissions/applicant/YOUR_APPLICANT_ID
Authorization: Bearer YOUR_TOKEN_HERE
```

**Response:**
```json
[
  {
    "id": "admission-guid",
    "status": "Created",
    "createdAt": "2024-11-16T10:00:00Z",
    "updatedAt": null,
    "educationProgram": {
      "name": "Computer Science",
      "code": "CS-001",
      "faculty": "Faculty of Computer Science"
    }
  }
]
```

#### 6. Manager Updates Status (Manager only)

**Request:**
```http
PUT http://localhost:5000/api/admissions/ADMISSION_ID/status
Authorization: Bearer MANAGER_TOKEN_HERE
Content-Type: application/json

{
  "status": 1
}
```

**Status Values:**
- `0` = Created
- `1` = UnderReview
- `2` = Accepted
- `3` = Rejected
- `4` = Closed

**✉️ Status change notification will be sent to applicant!**

---

## 📈 Workflow Diagram

```
┌─────────────────┐
│ 1. Register     │
│    User         │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 2. Login        │
│    Get Token    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 3. Create       │
│    Profile      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 4. Submit       │
│    Application  │ ──────► Email: "Application Received"
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 5. Manager      │
│    Reviews      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 6. Status       │
│    Updated      │ ──────► Email: "Status Changed to..."
└─────────────────┘
```

---

## 🔧 Background Jobs

The system runs **2 automated background jobs**:

### 1. External Data Sync Job
- **Frequency**: Every 6 hours (configurable)
- **Purpose**: Syncs faculties and education programs from external system
- **Configuration**: `Quartz:SyncCron` in `AdmissionService/appsettings.json`
- **External API**: `https://1c-mockup.kreosoft.space`

### 2. Cleanup Job
- **Frequency**: Daily at 3:00 AM
- **Purpose**: Auto-closes applications in "Under Review" status for more than 3 days
- **Configuration**: `Quartz:CleanupCron` in `AdmissionService/appsettings.json`

### 3. Email Notification Worker
- **Frequency**: Continuous background process
- **Purpose**: Monitors database for pending notifications and sends emails
- **Retry Logic**: Up to 3 attempts with delays

---

## 📂 Project Structure

```
university-admissions/
│
├── src/
│   ├── ApiGateway/              # Port 5000 - Entry point
│   │   ├── ocelot.json          # Routing configuration
│   │   └── Program.cs
│   │
│   ├── IdentityService/         # Port 5001 - Authentication
│   │   ├── Controllers/         # Auth endpoints
│   │   ├── Services/            # JWT service
│   │   └── Data/                # Identity database
│   │
│   ├── AdmissionService/        # Port 5002 - Business logic
│   │   ├── Controllers/         # API endpoints
│   │   ├── Features/            # CQRS handlers
│   │   ├── Entities/            # Database models
│   │   ├── Jobs/                # Background jobs
│   │   └── Services/            # Business services
│   │
│   ├── NotificationService/     # Background worker
│   │   ├── Services/            # Email service
│   │   ├── Worker.cs            # Main worker
│   │   └── Data/                # Notification database
│   │
│   └── Shared.Contracts/        # Shared DTOs and models
│
├── infra/
│   └── init-db-scripts/         # Database setup scripts
│       ├── 01-create-database.sql
│       ├── 02-create-tables.sql
│       ├── 03-seed-data.sql
│       ├── 04-constraints-indexes.sql
│       └── 05-seed-admin.sql
│
├── docs/                        # Documentation
│   ├── ARCHITECTURE.md
│   ├── API-SPECIFICATION.md
│   └── database-schema.drawio
│
├── postman-collection.json      # API testing collection
└── UniversityAdmissions.sln     # Solution file
```

---

## 🔐 Pre-configured Users

The system comes with pre-seeded admin accounts for testing:

### Admin Account
- **Email**: `admin@university.edu`
- **Password**: `Admin123!`
- **Role**: Admin

### Head Manager Account
- **Email**: `headmanager@university.edu`
- **Password**: `Manager123!`
- **Role**: Head Manager

### Faculty Manager Account
- **Email**: `csmanager@university.edu`
- **Password**: `Manager123!`
- **Role**: Faculty Manager (Computer Science)

---

## 📧 Email Notification Examples

### Application Submission
```
Subject: Application Received - Computer Science

Dear John Doe,

Your application for Computer Science program has been received 
and is currently under review.

Application ID: XXX-XXX
Program: Computer Science (Bachelor)
Faculty: Faculty of Computer Science
Submitted: November 16, 2024

You will receive email updates when your application status changes.

Best regards,
University Admissions Office
```

### Status Change
```
Subject: Application Status Update - Accepted

Dear John Doe,

Your application status has been updated.

Application ID: XXX-XXX
Program: Computer Science (Bachelor)
New Status: Accepted
Updated: November 16, 2024

Congratulations! Please check your email for next steps.

Best regards,
University Admissions Office
```

---

## 🛠️ Configuration Files

### Important Settings

**JWT Token Settings** (`appsettings.json` in all services):
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "UniversityAdmissionsIdentityService",
    "Audience": "UniversityAdmissionsClients"
  }
}
```

**External API Settings** (`AdmissionService/appsettings.json`):
```json
{
  "ExternalApi": {
    "BaseUrl": "https://1c-mockup.kreosoft.space",
    "Username": "student",
    "Password": "ny6gQnyn4ecbBrP9l1Fz"
  }
}
```

**Background Job Schedule** (`AdmissionService/appsettings.json`):
```json
{
  "Quartz": {
    "CleanupCron": "0 0 3 * * ?",      # Daily at 3 AM
    "SyncCron": "0 0 */6 * * ?"        # Every 6 hours
  }
}
```

---

## 📱 Building a Frontend

The backend API is ready for any frontend framework:

### Recommended Technologies
- **React** with TypeScript
- **Angular**
- **Vue.js**
- **Blazor** (for .NET integration)

### Key Integration Points

1. **Authentication Flow**:
   ```javascript
   // 1. Register user
   POST /api/auth/register
   
   // 2. Login
   POST /api/auth/login → Returns JWT token
   
   // 3. Store token
   localStorage.setItem('token', response.token)
   
   // 4. Use token in headers
   headers: { 'Authorization': `Bearer ${token}` }
   ```

2. **API Base URL**: `http://localhost:5000` (API Gateway)

3. **CORS**: Already configured to accept requests from any origin (update for production)

### Sample Frontend Screens Needed

1. **Login/Registration Page**
2. **Applicant Dashboard**
   - View profile
   - Submit new application
   - Track application status
3. **Manager Dashboard**
   - View pending applications
   - Update application status
   - Filter by faculty/program
4. **Admin Dashboard**
   - User management
   - System configuration

---

## 🚀 Deployment to Production

### Recommended Hosting Options

1. **Microsoft Azure**
   - Azure App Service for each service
   - Azure Database for PostgreSQL
   - Azure SendGrid for emails

2. **AWS (Amazon Web Services)**
   - EC2 or Elastic Beanstalk
   - RDS for PostgreSQL
   - SES for emails

3. **DigitalOcean**
   - Droplets for services
   - Managed PostgreSQL database
   - External SMTP service

### Pre-Deployment Checklist

- [ ] Update connection strings for production database
- [ ] Change JWT secret key (generate strong random key)
- [ ] Configure production email SMTP
- [ ] Enable HTTPS (SSL certificates)
- [ ] Update CORS to restrict to your domain only
- [ ] Set proper logging levels (Warning/Error only)
- [ ] Configure environment variables
- [ ] Run database migrations on production database
- [ ] Test all endpoints in production environment

---

## 📞 Support & Maintenance

### Logs Location

All services log to:
- **Console output** (visible when running)
- **Files**: `c:\temp\nlog-*.log`

### Common Issues & Solutions

**Issue: "Database connection failed"**
- Check PostgreSQL is running
- Verify connection string in `appsettings.json`
- Check firewall settings

**Issue: "Port already in use"**
```powershell
# Find process using port 5000
netstat -ano | findstr :5000

# Kill process
taskkill /PID <process-id> /F
```

**Issue: "Emails not sending"**
- Verify SMTP settings
- Check email credentials
- For Gmail, ensure app password is used
- Check NotificationService logs

**Issue: "JWT token invalid"**
- Ensure same JWT key in all services
- Check token hasn't expired (24 hour lifetime)
- Verify token format: `Bearer <token>`

### Database Backup

**Backup command:**
```powershell
pg_dump -U postgres -d university > backup_$(Get-Date -Format 'yyyyMMdd').sql
```

**Restore command:**
```powershell
psql -U postgres -d university < backup_20241116.sql
```

---

## 📊 System Requirements

### Development Environment
- **OS**: Windows 10/11, Linux, macOS
- **RAM**: 8 GB minimum, 16 GB recommended
- **Disk Space**: 5 GB free space
- **.NET 8 SDK**: Required
- **PostgreSQL 15**: Required

### Production Environment
- **CPU**: 2+ cores
- **RAM**: 4 GB minimum per service
- **Database**: PostgreSQL 15+
- **Network**: HTTPS enabled
- **Email**: SMTP server access

---

## 🎯 Key Features Summary

### ✅ What's Included

- Complete REST API backend
- User authentication and authorization
- Multi-role access control
- Applicant profile management
- Admission application workflow
- Document tracking
- Email notifications (automated)
- Background job processing
- External system integration
- Database with seed data
- API documentation (Swagger)
- Postman collection for testing
- Comprehensive documentation

### ❌ What's NOT Included

- Frontend application (web/mobile UI)
- File upload/storage for documents
- Payment processing
- SMS notifications
- Real-time chat/messaging
- Mobile apps

---

## 📝 Next Steps for Client

1. **Test the System**
   - Run all services locally
   - Test with Postman collection
   - Verify email notifications

2. **Review Documentation**
   - Read API specifications
   - Understand workflow
   - Check database schema

3. **Plan Frontend Development**
   - Choose framework (React/Angular/Vue)
   - Design UI/UX
   - Integrate with backend API

4. **Prepare for Deployment**
   - Choose hosting provider
   - Set up production database
   - Configure email service
   - Obtain SSL certificate

5. **Optional Enhancements**
   - File upload for documents
   - Real-time notifications (SignalR)
   - Advanced reporting
   - Two-factor authentication
   - Admin dashboard

---

## 📚 Additional Resources

### Documentation Files
- `README.md` - Complete technical documentation
- `ARCHITECTURE.md` - System architecture details
- `API-SPECIFICATION.md` - API endpoint specifications
- `postman-collection.json` - API testing collection

### Useful Links
- [.NET 8 Documentation](https://docs.microsoft.com/dotnet)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [JWT Introduction](https://jwt.io/introduction)

---

## ✨ Project Highlights

- **Technology**: Latest .NET 8.0 LTS
- **Architecture**: Microservices with clean separation
- **Code Quality**: CQRS pattern, SOLID principles
- **Security**: Industry-standard JWT authentication
- **Scalability**: Each service can scale independently
- **Maintainability**: Well-structured, documented code
- **Testing**: Postman collection included
- **Production-Ready**: Logging, health checks, error handling

---

## 📞 Contact & Support

For technical questions or issues:
1. Check logs in `c:\temp\nlog-*.log`
2. Review error messages in console output
3. Consult documentation files
4. Contact development team

---

**🎉 The system is complete and ready for deployment!**

**Last Updated**: November 16, 2024  
**Version**: 1.0  
**Built with**: .NET 8.0, PostgreSQL 15, JWT Authentication

---

*This document is confidential and intended for the client only.*
