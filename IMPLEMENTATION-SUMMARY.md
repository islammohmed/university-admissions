# Implementation Summary

This document describes the infrastructure and service improvements implemented for the University Admissions system.

## 3. Infrastructure - Docker Compose

### Changes Made

Updated `docker/docker-compose.yml` to include:

1. **PostgreSQL Database**
   - Image: `postgres:15`
   - Credentials: `app/secret`
   - Port: `5432`
   - Includes health checks

2. **RabbitMQ Message Broker**
   - Image: `rabbitmq:3-management`
   - Ports: `5672` (AMQP), `15672` (Management UI)
   - Management UI accessible at: http://localhost:15672
   - Default credentials: `guest/guest`

3. **All Services Updated**
   - API Gateway: Port `5000`
   - Identity Service: Port `5001`
   - Admission Service: Port `5002`
   - Notification Service: Background worker
   - All services include RabbitMQ configuration

### Running the System

```bash
# Start all services
docker-compose -f docker/docker-compose.yml up -d

# View logs
docker-compose -f docker/docker-compose.yml logs -f

# Stop all services
docker-compose -f docker/docker-compose.yml down
```

## 4. Identity Service Improvements

### 4.1 RefreshToken Model

Created `Models/RefreshToken.cs`:
- Stores refresh tokens in the database
- Linked to AspNetUsers via `UserId`
- Supports token rotation and revocation
- Tracks device information for security

**Fields:**
- `Id`: Unique identifier (Guid)
- `Token`: Base64-encoded secure random token
- `UserId`: Link to ApplicationUser
- `ExpiryDate`: Token expiration (30 days default)
- `Revoked`: Revocation status
- `DeviceInfo`: User-Agent information
- `ReplacedByTokenId`: Audit trail for token rotation

### 4.2 JWT + Refresh Token Logic

**Token Service** (`Services/TokenService.cs`):
- `GenerateAccessToken()`: Creates short-lived JWT (15 minutes)
- `GenerateRefreshTokenAsync()`: Creates long-lived refresh token (30 days)
- `ValidateRefreshTokenAsync()`: Validates refresh token from database
- `RotateRefreshTokenAsync()`: Implements token rotation on refresh
- `RevokeRefreshTokenAsync()`: Revokes specific token
- `RevokeAllUserTokensAsync()`: Revokes all user tokens (logout all devices)

**JWT Claims Include:**
- `sub`: User ID
- `email`: User email
- `name`: Username
- `role`: User role (Manager, Admin, etc.)
- `fullName`: User's full name

### 4.3 Authentication Endpoints

**POST /api/auth/register**
- Creates new user account
- Returns access token + refresh token
- Adds user to appropriate role

**POST /api/auth/login**
- Request: `{ "email", "password" }`
- Response: `{ "accessToken", "refreshToken", "expiresIn", ... }`
- Tracks device info via User-Agent header

**POST /api/auth/refresh**
- Request: `{ "refreshToken" }`
- Response: New access token + new refresh token
- Automatically rotates tokens (old token revoked)

**POST /api/auth/logout**
- Requires: Authorization header
- Request: `{ "refreshToken" }`
- Revokes the provided refresh token

**POST /api/auth/revoke-all**
- Requires: Authorization header
- Revokes all refresh tokens for the user
- Useful for "logout all devices" functionality

### 4.4 Database Seeding

**DbInitializer** (`Data/DbInitializer.cs`):
- Automatically runs on startup
- Creates roles: `Applicant`, `Manager`, `HeadManager`, `Admin`
- Seeds two default users with auto-generated secure passwords:
  - `admin@university.edu` - Admin role
  - `manager@university.edu` - Manager role
- **IMPORTANT**: Passwords are logged once during first run - save them!

### 4.5 Middleware & Configuration

**Program.cs Updates:**
- Registered `TokenService` as scoped service
- Added `DbInitializer` to startup pipeline
- Configured JWT authentication with zero clock skew
- Identity options configured for secure passwords

**Database Context Updates:**
- Added `RefreshTokens` DbSet
- Configured RefreshToken entity mappings
- Foreign key relationship to ApplicationUser

## 5. Admission Service Core

### 5.1 Domain Models

**Updated `Entities/Applicant.cs`:**
- Added `Status` field (enum: UnderReview, Accepted, Rejected, Closed)
- Added `AppliedAt` timestamp
- Existing fields: FullName, Email, BirthDate, etc.

**AdmissionStatus Enum:**
```csharp
public enum AdmissionStatus
{
    UnderReview,
    Accepted,
    Rejected,
    Closed
}
```

### 5.2 CQRS with MediatR

**Commands** (write operations):
- `CreateApplicantCommand`: Create new applicant
- Handlers are focused, testable, and handle one responsibility

**Queries** (read operations):
- `GetApplicantsQuery`: Fetch applicants with filtering and pagination
  - Supports filtering by Status
  - Pagination support (page number, page size)
  - Returns total count for UI pagination

**Benefits:**
- Separation of read and write concerns
- Easy to test handlers independently
- Clear intent with explicit command/query names
- Built-in logging and error handling

### 5.3 Background Jobs with Quartz.NET

**CleanupAdmissionsJob** (`Jobs/CleanupAdmissionsJob.cs`):
- **Purpose**: Auto-close stale applications
- **Logic**: Finds applicants with `Status == UnderReview` and `AppliedAt < 3 days ago`
- **Action**: Sets status to `Closed`
- **Schedule**: Daily at 3 AM (configurable via `Quartz:CleanupCron`)
- **Logging**: Detailed logs of closed applications

**ExternalDataSyncJob** (`Jobs/ExternalDataSyncJob.cs`):
- **Purpose**: Sync faculties/programs from external API
- **Logic**: 
  - Calls external API endpoint
  - Upserts Faculty records based on Code
  - Updates existing or creates new faculties
- **Schedule**: Every 6 hours (configurable via `Quartz:SyncCron`)
- **Error Handling**: HTTP errors logged, job continues
- **Future**: Can trigger notifications after sync

**Configuration** (appsettings.json):
```json
{
  "Quartz": {
    "CleanupCron": "0 0 3 * * ?",     // Daily at 3 AM
    "SyncCron": "0 0 */6 * * ?"       // Every 6 hours
  },
  "ExternalApi": {
    "Url": "https://external-api.example.com/faculties"
  }
}
```

**Cron Expression Format**: `second minute hour day month dayOfWeek`

### 5.4 Health Checks

**Health Controller** (`Controllers/HealthController.cs`):

**GET /health/live**
- Simple liveness probe
- Returns 200 if service is running
- No dependencies checked
- Use for Kubernetes/Docker liveness probes

**GET /health/ready**
- Readiness probe
- Checks database connectivity
- Returns 200 if ready, 503 if not ready
- Use for Kubernetes/Docker readiness probes

**Response Format:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "db",
      "status": "Healthy",
      "description": "DbContext health check",
      "duration": 45.2
    }
  ],
  "totalDuration": 45.2,
  "timestamp": "2025-11-11T12:00:00Z"
}
```

**Program.cs Configuration:**
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AdmissionDbContext>("db", tags: new[] { "ready" });
```

## Package Dependencies Added

### AdmissionService.csproj
- `Quartz` (3.9.0)
- `Quartz.Extensions.Hosting` (3.9.0)
- `Quartz.Extensions.DependencyInjection` (3.9.0)
- `Microsoft.Extensions.Diagnostics.HealthChecks` (8.0.11)
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (8.0.11)

## Testing the Implementation

### 1. Test Identity Service

```bash
# Register a new user
POST http://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123",
  "fullName": "Test User",
  "role": "Applicant"
}

# Login
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123"
}

# Refresh token
POST http://localhost:5001/api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "<refresh_token_from_login>"
}

# Logout
POST http://localhost:5001/api/auth/logout
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "refreshToken": "<refresh_token>"
}
```

### 2. Test Admission Service Health

```bash
# Liveness check
GET http://localhost:5002/health/live

# Readiness check
GET http://localhost:5002/health/ready
```

### 3. Monitor Background Jobs

Check logs for job executions:
```bash
docker-compose -f docker/docker-compose.yml logs -f admission-service
```

Look for:
- "Starting CleanupAdmissionsJob at ..."
- "Starting ExternalDataSyncJob at ..."

### 4. RabbitMQ Management

Access the management UI:
- URL: http://localhost:15672
- Username: `guest`
- Password: `guest`

## Database Migrations

After making these changes, you'll need to create migrations:

```bash
# Identity Service
cd src/IdentityService
dotnet ef migrations add AddRefreshTokens
dotnet ef database update

# Admission Service
cd src/AdmissionService
dotnet ef migrations add AddApplicantStatusFields
dotnet ef database update
```

## Security Considerations

1. **Token Expiration**:
   - Access tokens: 15 minutes (short-lived)
   - Refresh tokens: 30 days (long-lived)

2. **Token Rotation**:
   - Old refresh token revoked when new one issued
   - Prevents token replay attacks

3. **Device Tracking**:
   - User-Agent stored with refresh tokens
   - Enables device-specific token management

4. **Password Requirements**:
   - Minimum 6 characters
   - Requires digit, uppercase, lowercase
   - No special characters required (configurable)

5. **JWT Security**:
   - HMAC-SHA256 signing
   - Zero clock skew (no tolerance for expired tokens)
   - Issuer and audience validation

## Next Steps

1. **Add RabbitMQ Integration**:
   - Publish events from AdmissionService
   - Consume in NotificationService

2. **Enhance Background Jobs**:
   - Add more sophisticated error handling
   - Implement retry logic with exponential backoff
   - Add job execution history/audit trail

3. **Monitoring & Observability**:
   - Add structured logging (Serilog)
   - Implement distributed tracing (OpenTelemetry)
   - Add metrics (Prometheus)

4. **API Gateway**:
   - Configure Ocelot for health check endpoints
   - Add rate limiting
   - Implement circuit breakers

5. **Testing**:
   - Unit tests for CQRS handlers
   - Integration tests for background jobs
   - End-to-end tests for auth flow

## Troubleshooting

### Refresh token not working
- Check token hasn't expired (30 days)
- Verify token hasn't been revoked
- Ensure token exists in database

### Background jobs not running
- Check Quartz configuration in appsettings.json
- Verify cron expressions are valid
- Check logs for Quartz initialization

### Health checks failing
- Ensure database is accessible
- Check connection string
- Verify migrations have been applied

### Docker Compose issues
- Ensure ports aren't already in use
- Check docker logs: `docker-compose logs -f`
- Verify environment variables are correct
