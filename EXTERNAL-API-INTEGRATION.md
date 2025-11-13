# External System Integration Guide

## Overview

This document describes the integration with the external 1C-Mockup API system for synchronizing dictionary data (education levels, document types, faculties, and programs).

## External API Details

**Base URL**: `https://1c-mockup.kreosoft.space`

**Authentication**: Basic Authentication
- Username: `student`
- Password: `ny6gQnyn4ecbBrP9l1Fz`

## Available Endpoints

### 1. Education Levels
**GET** `/api/dictionary/education_levels`

Returns a list of education levels (Bachelor's, Master's, PhD, etc.).

**Response Example**:
```json
[
  {
    "id": 1,
    "name": "Bachelor's"
  },
  {
    "id": 2,
    "name": "Master's"
  }
]
```

### 2. Document Types
**GET** `/api/dictionary/document_types`

Returns a list of education document types with their associated education levels and next possible levels.

**Response Example**:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createTime": "2025-11-13T15:39:19.638Z",
    "name": "High School Diploma",
    "educationLevel": {
      "id": 0,
      "name": "Secondary"
    },
    "nextEducationLevels": [
      {
        "id": 1,
        "name": "Bachelor's"
      }
    ]
  }
]
```

### 3. Faculties
**GET** `/api/dictionary/faculties`

Returns a list of faculties available in the university.

**Response Example**:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createTime": "2025-11-13T15:39:38.859Z",
    "name": "Computer Science"
  }
]
```

### 4. Programs
**GET** `/api/dictionary/programs?page={page}&size={size}`

Returns a paginated list of education programs.

**Query Parameters**:
- `page` (integer, default: 1) - Page number
- `size` (integer, default: 10) - Number of elements per page

**Response Example**:
```json
{
  "programs": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createTime": "2025-11-13T15:40:01.771Z",
      "name": "Software Engineering",
      "code": "SE-2024",
      "language": "English",
      "educationForm": "Full-time",
      "faculty": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "createTime": "2025-11-13T15:40:01.772Z",
        "name": "Computer Science"
      },
      "educationLevel": {
        "id": 1,
        "name": "Bachelor's"
      }
    }
  ],
  "pagination": {
    "size": 10,
    "count": 5,
    "current": 1
  }
}
```

## Implementation

### Components

#### 1. DTOs (`DTOs/ExternalApiDtos.cs`)
- `ExternalEducationLevelDto`
- `ExternalDocumentTypeDto`
- `ExternalFacultyDto`
- `ExternalProgramDto`
- `ExternalProgramsResponseDto`
- `PaginationDto`

#### 2. Service Interface (`Services/IExternalDictionaryService.cs`)
Defines methods for accessing external API endpoints:
- `GetEducationLevelsAsync()`
- `GetDocumentTypesAsync()`
- `GetFacultiesAsync()`
- `GetProgramsAsync(page, size)`

#### 3. Service Implementation (`Services/ExternalDictionaryService.cs`)
Implements the interface with:
- Basic Authentication handling
- HTTP client factory for making requests
- Error handling and logging
- JSON deserialization

#### 4. Background Job (`Jobs/ExternalDataSyncJob.cs`)
Scheduled job that:
- Runs every 6 hours (configurable via `Quartz:SyncCron`)
- Synchronizes all dictionary data in order:
  1. Education Levels
  2. Document Types (with many-to-many relationships)
  3. Faculties
  4. Programs (with pagination support)
- Handles updates and new records
- Logs detailed information about the sync process

#### 5. API Controller (`Controllers/DictionaryController.cs`)
Provides endpoints for:
- Testing external API connection
- Viewing external data
- Manually triggering synchronization (admin only)

## Configuration

### appsettings.json
```json
{
  "ExternalApi": {
    "BaseUrl": "https://1c-mockup.kreosoft.space",
    "Username": "student",
    "Password": "ny6gQnyn4ecbBrP9l1Fz"
  },
  "Quartz": {
    "SyncCron": "0 0 */6 * * ?"
  }
}
```

### Cron Schedule
- `0 0 */6 * * ?` - Runs every 6 hours at the start of the hour (00:00, 06:00, 12:00, 18:00)
- Can be customized to run more or less frequently

## Testing Endpoints

### Local API Endpoints (AdmissionService)

#### Get Education Levels
```http
GET http://localhost:5001/api/dictionary/education-levels
```

#### Get Document Types
```http
GET http://localhost:5001/api/dictionary/document-types
```

#### Get Faculties
```http
GET http://localhost:5001/api/dictionary/faculties
```

#### Get Programs
```http
GET http://localhost:5001/api/dictionary/programs?page=1&size=20
```

#### Trigger Manual Sync (Admin Only)
```http
POST http://localhost:5001/api/dictionary/sync
Authorization: Bearer {admin_jwt_token}
```

## Synchronization Logic

### Order of Synchronization
1. **Education Levels** - Must be synced first as they're referenced by other entities
2. **Document Types** - References education levels and their relationships
3. **Faculties** - Independent but referenced by programs
4. **Programs** - References both faculties and education levels

### Update Strategy
- Uses external ID as the primary key
- If record exists: Updates all fields
- If record doesn't exist: Creates new record
- Handles many-to-many relationships for document types

### Pagination Handling
Programs are fetched in pages (default 100 per page) and synced incrementally until all pages are processed.

## Error Handling

- **HTTP Errors**: Logged with status code, sync continues with next entity type
- **Network Errors**: Logged and job execution is marked as failed
- **Data Errors**: Individual records that fail are logged, sync continues
- **Authentication Errors**: Logged with details about the failed authentication

## Monitoring

### Logs to Watch
- `"Starting ExternalDataSyncJob at {Time}"` - Job started
- `"Added new {entity}: {Name}"` - New record created
- `"Updated {entity}: {Name}"` - Existing record updated
- `"ExternalDataSyncJob completed successfully"` - Job finished successfully
- `"Error occurred while running ExternalDataSyncJob"` - Job failed

### Performance Metrics
Each sync logs:
- Number of education levels synced
- Number of document types synced
- Number of faculties synced
- Number of programs synced

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify credentials in appsettings.json
   - Check if external API is accessible

2. **Network Timeouts**
   - External API may be slow or unavailable
   - Check network connectivity
   - Consider increasing HttpClient timeout

3. **Data Inconsistencies**
   - External API may return incomplete data
   - Check logs for specific errors
   - Verify foreign key relationships

4. **Sync Not Running**
   - Check Quartz.NET configuration
   - Verify cron expression
   - Check application logs for scheduler errors

## Security Considerations

1. **Credentials Storage**
   - Stored in appsettings.json (should be in secrets manager for production)
   - Not committed to source control
   - Use environment variables or Azure Key Vault in production

2. **API Access**
   - Only the background job accesses external API automatically
   - Dictionary controller endpoints are public for viewing
   - Manual sync trigger requires Administrator role

## Future Enhancements

1. Add retry logic with exponential backoff
2. Implement incremental sync based on `createTime`
3. Add webhook support for real-time updates
4. Cache external API responses
5. Add metrics and alerting for sync failures
6. Implement data validation and reconciliation reports
