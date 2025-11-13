# API Specification - University Admissions System

**Version:** 1.0  
**Last Updated:** November 13, 2025  
**Base URL (via Gateway):** `http://localhost:5000`

---

## Table of Contents

1. [Authentication](#authentication)
2. [Identity Service API](#identity-service-api)
3. [Admission Service API](#admission-service-api)
4. [Health Check Endpoints](#health-check-endpoints)
5. [Error Responses](#error-responses)
6. [Data Models](#data-models)

---

## Authentication

All API endpoints (except registration and login) require JWT authentication.

### Request Headers
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### Token Expiration
- Tokens expire after **24 hours**
- Expired tokens will return `401 Unauthorized`

---

## Identity Service API

Base Path: `/api/auth`  
Direct URL: `http://localhost:5001` (bypassing gateway)

### 1. Register User

**Endpoint:** `POST /api/auth/register`  
**Description:** Register a new user (Applicant, Faculty Manager, or Head Manager)  
**Authentication:** Not required

#### Request Body
```json
{
  "email": "string",
  "password": "string",
  "fullName": "string",
  "role": 0
}
```

**Role Values:**
- `0` = Applicant
- `1` = FacultyManager
- `2` = HeadManager

**Password Requirements:**
- Minimum 6 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit

#### Success Response (201 Created)
```json
{
  "message": "User registered successfully"
}
```

#### Error Responses
- `400 Bad Request` - Validation error or user already exists
```json
{
  "errors": {
    "Email": ["Email is already registered"],
    "Password": ["Password must be at least 6 characters"]
  }
}
```

#### Example Request
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123",
    "fullName": "John Doe",
    "role": 0
  }'
```

---

### 2. Login

**Endpoint:** `POST /api/auth/login`  
**Description:** Authenticate user and receive JWT token  
**Authentication:** Not required

#### Request Body
```json
{
  "email": "string",
  "password": "string"
}
```

#### Success Response (200 OK)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Applicant",
  "expiresAt": "2025-11-14T12:00:00Z"
}
```

#### Error Responses
- `401 Unauthorized` - Invalid credentials
```json
{
  "message": "Invalid email or password"
}
```

#### Example Request
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123"
  }'
```

---

## Admission Service API

Base Path: `/api`  
Direct URL: `http://localhost:5002` (bypassing gateway)

### 3. Create Applicant Profile

**Endpoint:** `POST /api/applicants`  
**Description:** Create an applicant profile  
**Authentication:** Required (Applicant role)

#### Request Body
```json
{
  "fullName": "string",
  "email": "string",
  "birthDate": "2000-01-15",
  "gender": "Male",
  "citizenship": "string",
  "phoneNumber": "string"
}
```

**Gender Values:** `Male`, `Female`, `Other`

#### Success Response (201 Created)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "birthDate": "2000-01-15T00:00:00Z",
  "gender": "Male",
  "citizenship": "USA",
  "phoneNumber": "+1234567890",
  "userId": "abc123..."
}
```

#### Example Request
```bash
curl -X POST http://localhost:5000/api/applicants \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "birthDate": "2000-01-15",
    "gender": "Male",
    "citizenship": "USA",
    "phoneNumber": "+1234567890"
  }'
```

---

### 4. Get Applicant by ID

**Endpoint:** `GET /api/applicants/{id}`  
**Description:** Retrieve applicant profile by ID  
**Authentication:** Required

#### Path Parameters
- `id` (UUID) - Applicant identifier

#### Success Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "birthDate": "2000-01-15T00:00:00Z",
  "gender": "Male",
  "citizenship": "USA",
  "phoneNumber": "+1234567890",
  "userId": "abc123..."
}
```

#### Error Responses
- `404 Not Found` - Applicant not found

#### Example Request
```bash
curl -X GET http://localhost:5000/api/applicants/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer {token}"
```

---

### 5. Create Admission Application

**Endpoint:** `POST /api/admissions`  
**Description:** Submit a new admission application  
**Authentication:** Required (Applicant role)

#### Request Body
```json
{
  "applicantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
  "programs": [
    {
      "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
      "priority": 1
    },
    {
      "educationProgramId": "8fa85f64-5678-4562-b3fc-2c963f66afa6",
      "priority": 2
    }
  ]
}
```

**Priority:** 1 = First choice, 2 = Second choice, etc.

#### Success Response (201 Created)
```json
{
  "id": "9fa85f64-9999-4562-b3fc-2c963f66afa6",
  "applicantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "managerId": null,
  "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
  "status": "Created",
  "createdAt": "2025-11-13T12:00:00Z",
  "updatedAt": "2025-11-13T12:00:00Z",
  "programs": [
    {
      "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
      "priority": 1
    }
  ]
}
```

**Status Values:**
- `Created` - Initial state
- `UnderReview` - Manager is reviewing
- `Confirmed` - Application accepted
- `Rejected` - Application rejected
- `Closed` - Final state, no further changes allowed

#### Example Request
```bash
curl -X POST http://localhost:5000/api/admissions \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "applicantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
    "programs": [
      {
        "educationProgramId": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
        "priority": 1
      }
    ]
  }'
```

---

### 6. Get Admission Application by ID

**Endpoint:** `GET /api/admissions/{id}`  
**Description:** Retrieve admission application details  
**Authentication:** Required

#### Path Parameters
- `id` (UUID) - Admission application identifier

#### Success Response (200 OK)
```json
{
  "id": "9fa85f64-9999-4562-b3fc-2c963f66afa6",
  "applicant": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "John Doe",
    "email": "john.doe@example.com"
  },
  "manager": {
    "id": "manager-id",
    "fullName": "Jane Smith",
    "managerType": "FacultyManager"
  },
  "educationProgram": {
    "id": "7ea85f64-1234-4562-b3fc-2c963f66afa6",
    "name": "Computer Science",
    "code": "CS-B-2025"
  },
  "status": "UnderReview",
  "createdAt": "2025-11-13T12:00:00Z",
  "updatedAt": "2025-11-13T13:00:00Z",
  "programs": [...]
}
```

#### Error Responses
- `404 Not Found` - Admission not found

---

### 7. Update Admission Status

**Endpoint:** `PUT /api/admissions/{id}/status`  
**Description:** Update admission application status (Manager only)  
**Authentication:** Required (FacultyManager or HeadManager role)

#### Path Parameters
- `id` (UUID) - Admission application identifier

#### Request Body
```json
{
  "status": "Confirmed"
}
```

**Allowed Status Transitions:**
- `Created` → `UnderReview`
- `UnderReview` → `Confirmed` or `Rejected`
- `Confirmed` → `Closed`
- `Rejected` → `Closed`

#### Success Response (200 OK)
```json
{
  "id": "9fa85f64-9999-4562-b3fc-2c963f66afa6",
  "status": "Confirmed",
  "updatedAt": "2025-11-13T14:00:00Z"
}
```

#### Error Responses
- `400 Bad Request` - Invalid status transition
- `403 Forbidden` - Not authorized (not a manager)
- `404 Not Found` - Admission not found

#### Example Request
```bash
curl -X PUT http://localhost:5000/api/admissions/9fa85f64-9999-4562-b3fc-2c963f66afa6/status \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Confirmed"
  }'
```

---

### 8. Get Applicant's Admissions

**Endpoint:** `GET /api/admissions/applicant/{applicantId}`  
**Description:** Get all admission applications for an applicant  
**Authentication:** Required

#### Path Parameters
- `applicantId` (UUID) - Applicant identifier

#### Query Parameters
- `status` (optional) - Filter by status
- `pageNumber` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10) - Items per page

#### Success Response (200 OK)
```json
{
  "items": [
    {
      "id": "9fa85f64-9999-4562-b3fc-2c963f66afa6",
      "educationProgram": {
        "name": "Computer Science"
      },
      "status": "UnderReview",
      "createdAt": "2025-11-13T12:00:00Z"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

#### Example Request
```bash
curl -X GET "http://localhost:5000/api/admissions/applicant/3fa85f64-5717-4562-b3fc-2c963f66afa6?status=UnderReview" \
  -H "Authorization: Bearer {token}"
```

---

### 9. Get Dictionary Data

**Endpoint:** `GET /api/dictionary/faculties`  
**Description:** Get list of all faculties  
**Authentication:** Required

#### Success Response (200 OK)
```json
[
  {
    "id": "faculty-id-1",
    "name": "Faculty of Computer Science",
    "code": "FCS",
    "description": "..."
  }
]
```

**Other Dictionary Endpoints:**
- `GET /api/dictionary/education-levels` - Get education levels
- `GET /api/dictionary/education-programs` - Get education programs
- `GET /api/dictionary/education-programs/{facultyId}` - Get programs by faculty
- `GET /api/dictionary/document-types` - Get document types

---

## Health Check Endpoints

### Service Health Checks

All services implement health check endpoints:

**Endpoints:**
- `GET /health` - Basic liveness check
- `GET /health/ready` - Readiness check (includes database connectivity)

#### Success Response (200 OK)
```json
{
  "status": "Healthy",
  "results": {
    "db": {
      "status": "Healthy",
      "description": "Database connection successful"
    }
  },
  "totalDuration": "00:00:00.0234567"
}
```

#### Unhealthy Response (503 Service Unavailable)
```json
{
  "status": "Unhealthy",
  "results": {
    "db": {
      "status": "Unhealthy",
      "description": "Cannot connect to database"
    }
  }
}
```

---

## Error Responses

### Standard Error Format

All error responses follow this format:

```json
{
  "message": "Error description",
  "errors": {
    "FieldName": ["Validation error 1", "Validation error 2"]
  },
  "statusCode": 400
}
```

### HTTP Status Codes

- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Validation error or invalid request
- `401 Unauthorized` - Authentication required or token expired
- `403 Forbidden` - User doesn't have permission
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict (e.g., duplicate email)
- `500 Internal Server Error` - Server error

---

## Data Models

### Common Enums

#### AdmissionStatus
```csharp
public enum AdmissionStatus
{
    Created = 0,
    UnderReview = 1,
    Confirmed = 2,
    Rejected = 3,
    Closed = 4
}
```

#### UserRole
```csharp
public enum UserRole
{
    Applicant = 0,
    FacultyManager = 1,
    HeadManager = 2,
    Admin = 3
}
```

#### ManagerType
```csharp
public enum ManagerType
{
    FacultyManager = 0,
    HeadManager = 1
}
```

#### Gender
```csharp
public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}
```

---

## API Design Principles

This API follows **REST principles**:

1. **Resource-Based URLs** - URLs represent resources, not actions
   - ✅ `GET /api/applicants/{id}`
   - ❌ `GET /api/getApplicant?id={id}`

2. **HTTP Methods** - Proper use of HTTP verbs
   - `GET` - Retrieve resources
   - `POST` - Create new resources
   - `PUT` - Update existing resources
   - `DELETE` - Remove resources

3. **Stateless** - Each request contains all necessary information

4. **JSON Format** - All requests and responses use JSON

5. **Consistent Naming** - camelCase for JSON properties

6. **Pagination** - Large collections support pagination

7. **Filtering** - Query parameters for filtering results

8. **Versioning** - API version in URL (future consideration)

---

## Rate Limiting

Currently not implemented. Consider adding rate limiting in production:
- 100 requests per minute per IP
- 1000 requests per hour per authenticated user

---

## CORS Configuration

Current CORS policy (Development):
- Allows all origins (`*`)
- Allows all methods
- Allows all headers

**Production:** Configure specific allowed origins in `appsettings.json`

---

## Testing

Use the included Postman collection: `postman-collection.json`

**Import Steps:**
1. Open Postman
2. Import → Upload Files → Select `postman-collection.json`
3. Set environment variable `baseUrl` = `http://localhost:5000`
4. Set environment variable `token` after login

---

## Support

For API issues or questions, contact the development team or create an issue in the repository.

**Documentation Version:** 1.0  
**Last Updated:** November 13, 2025
