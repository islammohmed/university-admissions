# University Admissions System - Quick Start Guide

## 🚀 Quick Start (5 minutes)

### Option 1: Run with Docker (Recommended)

```bash
# 1. Navigate to project
cd university-admissions/docker

# 2. Start all services
docker-compose up --build

# 3. Wait for services to start (about 2-3 minutes)
# PostgreSQL will initialize with all tables and seed data

# 4. Test the system
curl http://localhost:5000/api/auth/register
```

**Services will be available at:**
- API Gateway: http://localhost:5000
- IdentityService: http://localhost:5001  
- AdmissionService: http://localhost:5002
- PostgreSQL: localhost:5432

### Option 2: Run Locally

```bash
# 1. Start PostgreSQL (local installation or Docker)
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15-alpine

# 2. Run database scripts
psql -h localhost -U postgres -d postgres -f infra/init-db-scripts/01-create-database.sql
psql -h localhost -U postgres -d postgres -f infra/init-db-scripts/02-create-tables.sql
psql -h localhost -U postgres -d postgres -f infra/init-db-scripts/03-seed-data.sql
psql -h localhost -U postgres -d postgres -f infra/init-db-scripts/04-constraints-indexes.sql

# 3. Update connection strings in appsettings.json files to use localhost

# 4. Run each service
dotnet run --project src/IdentityService/IdentityService.csproj
dotnet run --project src/AdmissionService/AdmissionService.csproj
dotnet run --project src/NotificationService/NotificationService.csproj
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

## 📝 Test the API

### 1. Register an Applicant

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!",
    "fullName": "John Doe",
    "role": 0
  }'
```

### 2. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

**Save the token from response!**

### 3. Create Applicant Profile

```bash
curl -X POST http://localhost:5000/api/applicants \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "birthDate": "2000-01-15T00:00:00Z",
    "gender": 0,
    "citizenship": "USA",
    "phoneNumber": "+1234567890"
  }'
```

### 4. Submit Admission Application

```bash
curl -X POST http://localhost:5000/api/admissions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "applicantId": "APPLICANT_ID_FROM_STEP_3",
    "educationProgramId": "e1111111-1111-1111-1111-111111111111"
  }'
```

### 5. Check Admission Status

```bash
curl http://localhost:5000/api/admissions/applicant/APPLICANT_ID \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 🔑 User Roles

- **Role 0: Applicant** - Can create profile and submit applications
- **Role 1: FacultyManager** - Can review and update admission statuses for their faculty
- **Role 2: HeadManager** - Can manage all admissions across all faculties

## 📊 Pre-seeded Data

### Education Levels
- Bachelor (ID: 11111111-1111-1111-1111-111111111111)
- Master (ID: 22222222-2222-2222-2222-222222222222)
- PhD (ID: 33333333-3333-3333-3333-333333333333)

### Faculties
- Faculty of Computer Science (ID: f1111111-1111-1111-1111-111111111111)
- Faculty of Engineering (ID: f2222222-2222-2222-2222-222222222222)
- Faculty of Business Administration (ID: f3333333-3333-3333-3333-333333333333)

### Education Programs
- Computer Science - CS-001 (ID: e1111111-1111-1111-1111-111111111111)
- Software Engineering - SE-001 (ID: e2222222-2222-2222-2222-222222222222)
- Data Science - DS-001 (ID: e3333333-3333-3333-3333-333333333333)
- Mechanical Engineering - ME-001 (ID: e4444444-4444-4444-4444-444444444444)
- Business Administration - BA-001 (ID: e5555555-5555-5555-5555-555555555555)

## 🛠️ Common Commands

### Docker

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild after code changes
docker-compose up --build

# Remove volumes (fresh start)
docker-compose down -v
```

### Database

```bash
# Connect to PostgreSQL
docker exec -it postgres-db psql -U postgres

# List databases
\l

# Connect to admission_service
\c admission_service

# List tables
\dt

# Query data
SELECT * FROM "Applicants";
SELECT * FROM "ApplicantAdmissions";
```

### .NET Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run specific service
dotnet run --project src/IdentityService/IdentityService.csproj

# Create migration (AdmissionService)
cd src/AdmissionService
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

## ⚠️ Troubleshooting

### Port Already in Use
```bash
# Check what's using the port
netstat -ano | findstr :5000

# Kill the process (Windows)
taskkill /PID <process_id> /F
```

### Docker Issues
```bash
# Clean everything
docker-compose down -v
docker system prune -a

# Restart Docker Desktop
```

### Database Connection Failed
- Ensure PostgreSQL is running
- Check connection strings in appsettings.json
- Verify credentials (default: postgres/postgres)

### Email Not Sending
- Update Email settings in NotificationService appsettings.json
- For Gmail: Use app password, not regular password
- Check SMTP server and port settings

## 📚 Next Steps

1. **Explore Swagger UI:**
   - IdentityService: http://localhost:5001/swagger
   - AdmissionService: http://localhost:5002/swagger

2. **Add a Faculty Manager:**
   - Register with role: 1
   - Test updating admission statuses

3. **Test Notifications:**
   - Configure email settings
   - Watch logs: `docker-compose logs -f notification-service`

4. **Customize:**
   - Add new education programs
   - Modify admission workflow
   - Add document upload functionality

## 💡 Tips

- Use Postman or Insomnia for easier API testing
- Check Docker logs if services don't start
- Database is automatically initialized on first run
- JWT tokens expire after 24 hours
- All times are in UTC

---

**Need help?** Check the main README.md for detailed documentation.
