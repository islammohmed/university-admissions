# 🎓 University Admissions System - Quick Reference Card

## ⚡ Quick Start (3 Minutes)

### Step 1: Install Prerequisites
```powershell
# Install .NET 8 SDK (if not installed)
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# Install PostgreSQL 15 (if not installed)
# Download from: https://www.postgresql.org/download/windows/
# Use password: 1234
```

### Step 2: Setup Database
```powershell
# Open pgAdmin or psql
# Run these scripts in order:
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\infra\init-db-scripts"

psql -U postgres -c "CREATE DATABASE university;"
psql -U postgres -d university -f 02-create-tables.sql
psql -U postgres -d university -f 03-seed-data.sql
psql -U postgres -d university -f 04-constraints-indexes.sql
```

### Step 3: Configure Email
Edit `src\NotificationService\appsettings.json`:
```json
{
  "Email": {
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password"
  }
}
```

### Step 4: Run Services (4 Terminal Windows)

**Terminal 1 - API Gateway:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\ApiGateway"
dotnet run
```

**Terminal 2 - Identity Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\IdentityService"
dotnet run
```

**Terminal 3 - Admission Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\AdmissionService"
dotnet run
```

**Terminal 4 - Notification Service:**
```powershell
cd "c:\Users\Islam\Desktop\FreeLAnce\.NET Project\university-admissions\src\NotificationService"
dotnet run
```

### Step 5: Test
Open: http://localhost:5000/health

---

## 🔑 API Quick Reference

### Base URL
```
http://localhost:5000
```

### 1. Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe",
  "role": 0
}
```
**Roles**: 0=Applicant, 1=FacultyManager, 2=HeadManager

### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```
**Returns**: `{ "token": "...", "userId": "...", ... }`

### 3. Create Applicant Profile
```http
POST /api/applicants
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "user@example.com",
  "birthDate": "2000-01-15T00:00:00Z",
  "gender": 0,
  "citizenship": "USA",
  "phoneNumber": "+1234567890"
}
```

### 4. Submit Application
```http
POST /api/admissions
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "applicantId": "APPLICANT_GUID",
  "educationProgramId": "e1111111-1111-1111-1111-111111111111"
}
```

### 5. Get Applicant's Applications
```http
GET /api/admissions/applicant/APPLICANT_ID
Authorization: Bearer YOUR_TOKEN
```

### 6. Update Status (Manager only)
```http
PUT /api/admissions/ADMISSION_ID/status
Authorization: Bearer MANAGER_TOKEN
Content-Type: application/json

{
  "status": 1
}
```
**Status**: 0=Created, 1=UnderReview, 2=Accepted, 3=Rejected, 4=Closed

---

## 📊 Pre-seeded Data

### Test Accounts
| Email | Password | Role |
|-------|----------|------|
| admin@university.edu | Admin123! | Admin |
| headmanager@university.edu | Manager123! | Head Manager |
| csmanager@university.edu | Manager123! | Faculty Manager (CS) |

### Education Programs
| ID | Name | Code | Faculty |
|----|------|------|---------|
| e1111111-1111-1111-1111-111111111111 | Computer Science | CS-001 | Computer Science |
| e2222222-2222-2222-2222-222222222222 | Software Engineering | SE-001 | Computer Science |
| e3333333-3333-3333-3333-333333333333 | Data Science | DS-001 | Computer Science |
| e4444444-4444-4444-4444-444444444444 | Mechanical Engineering | ME-001 | Engineering |
| e5555555-5555-5555-5555-555555555555 | Business Administration | BA-001 | Business |

---

## 🎯 System Ports

| Service | Port | Purpose |
|---------|------|---------|
| API Gateway | 5000 | Main entry point |
| Identity Service | 5001 | Authentication |
| Admission Service | 5002 | Business logic |
| PostgreSQL | 5432 | Database |
| Notification Service | - | Background worker |

---

## 📂 Key Configuration Files

### Database Connection
File: `src/[ServiceName]/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=university;Username=postgres;Password=1234"
  }
}
```

### Email Settings
File: `src/NotificationService/appsettings.json`
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

### JWT Settings
File: `src/IdentityService/appsettings.json` and `src/AdmissionService/appsettings.json`
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "UniversityAdmissionsIdentityService",
    "Audience": "UniversityAdmissionsClients"
  }
}
```

---

## 🔧 Common Commands

### Database Commands
```powershell
# Connect to database
psql -U postgres -d university

# List tables
\dt

# Query applicants
SELECT * FROM "Applicants";

# Query admissions
SELECT * FROM "ApplicantAdmissions";

# Backup database
pg_dump -U postgres -d university > backup.sql

# Restore database
psql -U postgres -d university < backup.sql
```

### .NET Commands
```powershell
# Build solution
dotnet build

# Run specific service
dotnet run --project src/AdmissionService/AdmissionService.csproj

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

### Check Port Usage
```powershell
# Check what's using a port
netstat -ano | findstr :5000

# Kill process by PID
taskkill /PID <process-id> /F
```

---

## ⚠️ Troubleshooting

### Issue: Port Already in Use
```powershell
# Find and kill the process
netstat -ano | findstr :5000
taskkill /PID <process-id> /F
```

### Issue: Database Connection Failed
- ✅ Check PostgreSQL is running
- ✅ Verify password in appsettings.json (should be `1234`)
- ✅ Check port 5432 is not blocked

### Issue: Email Not Sending
- ✅ Update email settings in NotificationService/appsettings.json
- ✅ For Gmail: Use app password, not regular password
- ✅ Enable "Less secure app access" or use app password
- ✅ Check logs in `c:\temp\nlog-*.log`

### Issue: 401 Unauthorized
- ✅ Ensure token is included: `Authorization: Bearer <token>`
- ✅ Check token hasn't expired (24 hours)
- ✅ Verify JWT key matches across all services

---

## 📊 Application Workflow

```
1. Register → 2. Login → 3. Get Token → 4. Create Profile → 5. Submit Application
                                                                         ↓
                                                                   Email Sent ✉️
                                                                         ↓
6. Manager Reviews → 7. Updates Status → 8. Applicant Notified via Email ✉️
```

---

## 🎯 Status Codes

| Status | Value | Description |
|--------|-------|-------------|
| Created | 0 | Application submitted |
| UnderReview | 1 | Under review by manager |
| Accepted | 2 | Application accepted ✅ |
| Rejected | 3 | Application rejected ❌ |
| Closed | 4 | Application closed |

---

## 📧 Email Notifications

Automatic emails are sent when:
- ✉️ Application is submitted (Status: Created)
- ✉️ Status changes to Under Review
- ✉️ Application is Accepted
- ✉️ Application is Rejected
- ✉️ Application is Closed

---

## 🚀 Background Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| Data Sync | Every 6 hours | Sync faculties and programs from external API |
| Cleanup | Daily at 3 AM | Auto-close stale applications (>3 days in review) |
| Email Worker | Continuous | Monitor and send pending email notifications |

---

## 📱 Swagger Documentation

Access interactive API documentation:
- Identity Service: http://localhost:5001/swagger
- Admission Service: http://localhost:5002/swagger

---

## 🔐 Security Notes

- JWT tokens expire after 24 hours
- Passwords must meet: 8+ chars, uppercase, lowercase, digit, special char
- All endpoints (except register/login) require authentication
- Managers can only update statuses, not create applications
- Applicants can only view their own data

---

## 📞 Support Files

| File | Purpose |
|------|---------|
| CLIENT-DELIVERY-GUIDE.md | Complete setup and usage guide |
| postman-collection.json | API testing collection |
| README.md | Technical documentation |
| ARCHITECTURE.md | System architecture details |

---

## ✨ Features Checklist

### ✅ Completed Features
- [x] User registration and authentication
- [x] JWT token-based security
- [x] Multi-role access control (Applicant, Manager, Admin)
- [x] Applicant profile management
- [x] Admission application submission
- [x] Document tracking (metadata)
- [x] Status workflow management
- [x] Email notifications (automated)
- [x] Background job processing
- [x] External API integration
- [x] Database with seed data
- [x] API documentation (Swagger)
- [x] Health check endpoints
- [x] Comprehensive logging

### ⏳ Not Included (Future Enhancements)
- [ ] Frontend application (web/mobile UI)
- [ ] File upload for documents
- [ ] Real-time notifications (WebSocket)
- [ ] Payment processing
- [ ] SMS notifications
- [ ] Two-factor authentication (2FA)
- [ ] Advanced reporting and analytics

---

## 🎓 Quick Tips

1. **Always use API Gateway** (port 5000) as entry point
2. **Save your JWT token** after login - you'll need it for all requests
3. **Check health endpoints** to verify services are running
4. **Use Postman collection** for easier testing
5. **Check logs** in `c:\temp\nlog-*.log` for troubleshooting
6. **Run all 4 services** for full functionality
7. **Configure email** to test notifications

---

**🚀 Ready to start? Follow the Quick Start section at the top!**

**For detailed information, see: CLIENT-DELIVERY-GUIDE.md**

---

*Last Updated: November 16, 2024*
