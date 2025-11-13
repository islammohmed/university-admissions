# Database Migration Guide

After implementing the new features, you need to create and apply database migrations.

## Prerequisites

Make sure you have the EF Core tools installed:

```bash
dotnet tool install --global dotnet-ef
# or update if already installed
dotnet tool update --global dotnet-ef
```

## Identity Service Migrations

The Identity Service needs a new migration for the `RefreshTokens` table:

```bash
# Navigate to IdentityService directory
cd src/IdentityService

# Create migration
dotnet ef migrations add AddRefreshTokens --context ApplicationDbContext

# Apply migration
dotnet ef database update --context ApplicationDbContext
```

**What this adds:**
- `RefreshTokens` table with columns:
  - Id (Guid, Primary Key)
  - Token (string, unique index)
  - UserId (string, Foreign Key to AspNetUsers)
  - ExpiryDate (DateTime)
  - Revoked (bool)
  - DeviceInfo (string, nullable)
  - CreatedAt (DateTime)
  - ReplacedByTokenId (Guid, nullable)

## Admission Service Migrations

The Admission Service needs migrations for the new Applicant and Faculty fields:

```bash
# Navigate to AdmissionService directory
cd src/AdmissionService

# Create migration
dotnet ef migrations add AddApplicantStatusAndFacultyFields --context AdmissionDbContext

# Apply migration
dotnet ef database update --context AdmissionDbContext
```

**What this adds:**

### Applicant table updates:
- `Status` (int, default: 1 = UnderReview)
- `AppliedAt` (DateTime)

### Faculty table updates:
- `Code` (string, for external API sync)
- `Description` (string, nullable)

## Using Docker

If you're using Docker Compose, the migrations will need to be applied inside the containers:

```bash
# Start the services
docker-compose -f docker/docker-compose.yml up -d

# Apply Identity Service migrations
docker exec -it identity-service dotnet ef database update --context ApplicationDbContext

# Apply Admission Service migrations
docker exec -it admission-service dotnet ef database update --context AdmissionDbContext
```

## Alternative: Automatic Migrations on Startup

You can add automatic migration application in `Program.cs`:

### IdentityService Program.cs
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}
```

### AdmissionService Program.cs
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
    await context.Database.MigrateAsync();
}
```

**Note:** Auto-migrations are convenient for development but not recommended for production. In production, migrations should be applied as part of a controlled deployment process.

## Verify Migrations

After applying migrations, verify the database schema:

```bash
# List migrations (Identity Service)
cd src/IdentityService
dotnet ef migrations list --context ApplicationDbContext

# List migrations (Admission Service)
cd src/AdmissionService
dotnet ef migrations list --context AdmissionDbContext
```

## Rollback (if needed)

If you need to rollback a migration:

```bash
# Rollback to previous migration
dotnet ef database update <PreviousMigrationName> --context <ContextName>

# Remove the last migration file
dotnet ef migrations remove --context <ContextName>
```

## Troubleshooting

### "No migrations configuration type was found"
- Make sure you're in the correct directory (where the .csproj file is)
- Ensure the project builds successfully

### "Unable to create an object of type 'DbContext'"
- Check that your connection string is configured in appsettings.json
- Ensure the database server is running

### "A network-related or instance-specific error"
- Verify PostgreSQL is running
- Check connection string credentials
- If using Docker, ensure containers are running and healthy

### "The database does not exist"
- PostgreSQL will create the database if it doesn't exist
- If using Docker, the init scripts should create databases
- Manually create: `CREATE DATABASE identity_db;` and `CREATE DATABASE admission_service;`

## Production Deployment Checklist

1. ✅ Test migrations on a development database first
2. ✅ Backup production database before applying migrations
3. ✅ Review migration SQL scripts: `dotnet ef migrations script`
4. ✅ Apply migrations during maintenance window
5. ✅ Verify application functionality after migration
6. ✅ Monitor logs for any database-related errors
