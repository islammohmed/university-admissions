# Notification Service Implementation Guide

## Overview
The Notification Service handles email notifications and stores notification history. It supports both HTTP API endpoints (6.1) and event-driven messaging via RabbitMQ + MassTransit (6.2).

## Features Implemented

### 6.1 HTTP Approach ✅
The service exposes REST API endpoints for sending notifications:

**POST /api/notifications/send**
```json
{
  "email": "user@example.com",
  "subject": "Application Status Update",
  "body": "<html>Your application has been updated...</html>",
  "applicantId": "guid-here"
}
```

The service:
1. Creates a notification record in the database
2. Attempts to send email via SMTP (MailKit)
3. Updates notification status (Sent/Failed)
4. Returns notification ID and status

**GET /api/notifications/applicant/{applicantId}**
- Retrieves notification history for a specific applicant

**GET /api/notifications**
- Retrieves all notifications (paginated)

### 6.2 Event-Driven Approach ✅
The service consumes RabbitMQ messages via MassTransit:

**Consumer: ApplicantStatusChangedConsumer**
- Listens for `ApplicantStatusChangedEvent`
- Automatically triggered when admission status changes
- Generates personalized email content
- Sends email and stores notification record

**Event Structure:**
```csharp
public class ApplicantStatusChangedEvent
{
    public Guid ApplicantId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public string ApplicantEmail { get; set; }
    public string ApplicantName { get; set; }
}
```

## Database Schema

**Notification Entity:**
```csharp
public class Notification
{
    public Guid Id { get; set; }
    public string To { get; set; }              // Email recipient
    public string Subject { get; set; }          // Email subject
    public string Body { get; set; }            // Email body (HTML)
    public string? ApplicantId { get; set; }    // Related applicant
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string Status { get; set; }          // Pending, Sent, Failed
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## Architecture

### Services
1. **NotificationsController** - HTTP API endpoints
2. **EmailService** - SMTP email sending (MailKit)
3. **Worker** - Background service for retry logic
4. **ApplicantStatusChangedConsumer** - RabbitMQ event consumer

### Technology Stack
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **MailKit** - Email sending
- **MassTransit** - Message bus abstraction
- **RabbitMQ** - Message broker
- **Swagger** - API documentation

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=notification_service;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "FromName": "University Admissions System",
    "FromAddress": "noreply@university.edu",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Email Setup (Gmail Example)
1. Enable 2-factor authentication on Gmail
2. Generate an App Password:
   - Go to Google Account → Security
   - Select "2-Step Verification"
   - Scroll to "App passwords"
   - Generate password for "Mail" application
3. Update `Email:Username` and `Email:Password` in appsettings

## Database Migrations

### Create Initial Migration
```powershell
cd src/NotificationService
dotnet ef migrations add InitialCreate
```

### Apply Migration
```powershell
dotnet ef database update
```

### Auto-Migration on Startup
The service automatically applies pending migrations in Development environment:
```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

## Integration with Admission Service

The AdmissionService publishes events when admission status changes:

**UpdateAdmissionStatusHandler.cs:**
```csharp
await _publishEndpoint.Publish(new ApplicantStatusChangedEvent
{
    ApplicantId = admission.ApplicantId,
    OldStatus = oldStatus,
    NewStatus = admission.Status.ToString(),
    Timestamp = DateTime.UtcNow,
    ApplicantEmail = admission.Applicant.Email,
    ApplicantName = admission.Applicant.FullName
}, cancellationToken);
```

## API Gateway Configuration

The API Gateway (Ocelot) routes requests to NotificationService:

```json
{
  "DownstreamPathTemplate": "/api/notifications/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    { "Host": "notification-service", "Port": 8080 }
  ],
  "UpstreamPathTemplate": "/api/notifications/{everything}",
  "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

## Running the Service

### Standalone (Development)
```powershell
cd src/NotificationService
dotnet run
```

Service will be available at: `http://localhost:5003`
Swagger UI: `http://localhost:5003/swagger`

### Docker Compose
```powershell
cd docker
docker-compose up -d
```

Services:
- **NotificationService**: `http://localhost:5003`
- **API Gateway**: `http://localhost:5000`
- **RabbitMQ Management**: `http://localhost:15672` (guest/guest)

## Testing

### Test HTTP Endpoint
```powershell
# Using PowerShell
$body = @{
    email = "test@example.com"
    subject = "Test Notification"
    body = "<h1>Hello</h1><p>This is a test email.</p>"
    applicantId = "00000000-0000-0000-0000-000000000000"
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "http://localhost:5003/api/notifications/send" -Body $body -ContentType "application/json"
```

### Test via API Gateway
```powershell
# Get JWT token first from IdentityService
$token = "your-jwt-token"

Invoke-RestMethod -Method Post `
    -Uri "http://localhost:5000/api/notifications/send" `
    -Headers @{ Authorization = "Bearer $token" } `
    -Body $body `
    -ContentType "application/json"
```

### Monitor RabbitMQ
1. Open RabbitMQ Management UI: `http://localhost:15672`
2. Login with guest/guest
3. Check Queues tab to see message flow
4. View Exchanges to see event routing

## Background Worker

The Worker service runs every 30 seconds to:
1. Find notifications with Status = "Pending" or "Failed" (retry < 3)
2. Attempt to send emails
3. Update status to "Sent" or increment retry count

This provides resilience for transient email sending failures.

## Logging

The service uses ILogger for structured logging:

```csharp
_logger.LogInformation("Processing notification {Id} for {Email}", notification.Id, notification.To);
_logger.LogError(ex, "Error processing ApplicantStatusChangedEvent for Applicant {ApplicantId}", evt.ApplicantId);
```

Configure logging levels in appsettings.json:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "MassTransit": "Debug"
    }
  }
}
```

## Troubleshooting

### Email Not Sending
1. Check SMTP credentials in appsettings.json
2. Verify firewall allows outbound connections on port 587
3. Check EmailService logs for detailed errors
4. Test SMTP connection manually using telnet

### RabbitMQ Connection Issues
1. Verify RabbitMQ is running: `docker ps | findstr rabbitmq`
2. Check RabbitMQ logs: `docker logs rabbitmq`
3. Verify connection string in appsettings.json
4. Check MassTransit logs for connection errors

### Events Not Being Consumed
1. Verify AdmissionService is publishing events (check logs)
2. Check RabbitMQ queues for pending messages
3. Ensure consumer is registered in Program.cs
4. Verify exchange/queue bindings in RabbitMQ UI

## Next Steps

### Production Recommendations
1. **Use a dedicated email service**: SendGrid, AWS SES, or Mailgun
2. **Add email templates**: Use Razor or Handlebars for HTML templates
3. **Implement rate limiting**: Prevent email flooding
4. **Add dead letter queue**: Handle persistent failures
5. **Monitor email delivery**: Track bounces and complaints
6. **Add notification preferences**: Let users opt-out of certain notifications
7. **Implement webhook callbacks**: For email delivery status
8. **Add SMS support**: For critical notifications

### Enhancements
- Add support for notification templates
- Implement notification scheduling
- Add support for multiple channels (SMS, Push, Slack)
- Implement notification preferences per user
- Add batch notification sending
- Implement notification analytics

## API Documentation

Full API documentation is available via Swagger when running in Development mode:
- Standalone: `http://localhost:5003/swagger`
- Via Gateway: `http://localhost:5000/swagger`
