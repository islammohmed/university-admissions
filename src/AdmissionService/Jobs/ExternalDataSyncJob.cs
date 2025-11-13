using AdmissionService.Data;
using AdmissionService.Entities;
using AdmissionService.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AdmissionService.Jobs;

public class ExternalDataSyncJob : IJob
{
    private readonly ILogger<ExternalDataSyncJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExternalDataSyncJob(
        ILogger<ExternalDataSyncJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting ExternalDataSyncJob at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
        var externalService = scope.ServiceProvider.GetRequiredService<IExternalDictionaryService>();

        var cancellationToken = context.CancellationToken;

        try
        {
            // 1. Sync Education Levels
            var educationLevelsCount = await SyncEducationLevels(dbContext, externalService, cancellationToken);
            
            // 2. Sync Document Types (depends on Education Levels)
            var documentTypesCount = await SyncDocumentTypes(dbContext, externalService, cancellationToken);
            
            // 3. Sync Faculties
            var facultiesCount = await SyncFaculties(dbContext, externalService, cancellationToken);
            
            // 4. Sync Programs (depends on Faculties and Education Levels)
            var programsCount = await SyncPrograms(dbContext, externalService, cancellationToken);

            _logger.LogInformation(
                "ExternalDataSyncJob completed successfully. " +
                "Synced - Education Levels: {LevelsCount}, Document Types: {DocTypesCount}, " +
                "Faculties: {FacultiesCount}, Programs: {ProgramsCount}",
                educationLevelsCount, documentTypesCount, facultiesCount, programsCount);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while syncing external data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while running ExternalDataSyncJob");
            throw;
        }
    }

    private async Task<int> SyncEducationLevels(
        AdmissionDbContext dbContext,
        IExternalDictionaryService externalService,
        CancellationToken cancellationToken)
    {
        var externalLevels = await externalService.GetEducationLevelsAsync(cancellationToken);
        var syncedCount = 0;

        foreach (var externalLevel in externalLevels)
        {
            // Convert int ID to Guid using deterministic approach
            var levelGuid = ConvertIntToGuid(externalLevel.Id);
            
            var level = await dbContext.EducationLevels
                .FirstOrDefaultAsync(l => l.Id == levelGuid, cancellationToken);

            if (level == null)
            {
                level = new EducationLevel
                {
                    Id = levelGuid,
                    Name = externalLevel.Name
                };
                dbContext.EducationLevels.Add(level);
                _logger.LogInformation("Added new education level: {Name} (ID: {Id})", level.Name, level.Id);
            }
            else
            {
                level.Name = externalLevel.Name;
                _logger.LogDebug("Updated education level: {Name} (ID: {Id})", level.Name, level.Id);
            }

            syncedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return syncedCount;
    }
    
    private static Guid ConvertIntToGuid(int value)
    {
        // Create a deterministic Guid from an integer
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private async Task<int> SyncDocumentTypes(
        AdmissionDbContext dbContext,
        IExternalDictionaryService externalService,
        CancellationToken cancellationToken)
    {
        var externalDocTypes = await externalService.GetDocumentTypesAsync(cancellationToken);
        var syncedCount = 0;

        foreach (var externalDocType in externalDocTypes)
        {
            var docType = await dbContext.EducationDocumentTypes
                .Include(dt => dt.BelongsToLevel)
                .Include(dt => dt.NextAvailableLevels)
                .FirstOrDefaultAsync(dt => dt.Id == externalDocType.Id, cancellationToken);

            // Convert external education level int ID to Guid
            var belongsToLevelGuid = ConvertIntToGuid(externalDocType.EducationLevel.Id);

            if (docType == null)
            {
                docType = new EducationDocumentType
                {
                    Id = externalDocType.Id,
                    Name = externalDocType.Name,
                    BelongsToLevelId = belongsToLevelGuid
                };
                dbContext.EducationDocumentTypes.Add(docType);
                _logger.LogInformation("Added new document type: {Name} (ID: {Id})", docType.Name, docType.Id);
            }
            else
            {
                docType.Name = externalDocType.Name;
                docType.BelongsToLevelId = belongsToLevelGuid;
                _logger.LogDebug("Updated document type: {Name} (ID: {Id})", docType.Name, docType.Id);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Sync NextAvailableLevels many-to-many relationship
            var currentNextLevels = await dbContext.Entry(docType)
                .Collection(dt => dt.NextAvailableLevels)
                .Query()
                .ToListAsync(cancellationToken);

            var newNextLevelGuids = externalDocType.NextEducationLevels
                .Select(l => ConvertIntToGuid(l.Id))
                .ToHashSet();

            // Remove old relationships
            foreach (var level in currentNextLevels)
            {
                if (!newNextLevelGuids.Contains(level.Id))
                {
                    docType.NextAvailableLevels.Remove(level);
                }
            }

            // Add new relationships
            foreach (var nextLevelGuid in newNextLevelGuids)
            {
                if (!currentNextLevels.Any(l => l.Id == nextLevelGuid))
                {
                    var level = await dbContext.EducationLevels.FindAsync(new object[] { nextLevelGuid }, cancellationToken);
                    if (level != null)
                    {
                        docType.NextAvailableLevels.Add(level);
                    }
                }
            }

            syncedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return syncedCount;
    }

    private async Task<int> SyncFaculties(
        AdmissionDbContext dbContext,
        IExternalDictionaryService externalService,
        CancellationToken cancellationToken)
    {
        var externalFaculties = await externalService.GetFacultiesAsync(cancellationToken);
        var syncedCount = 0;

        foreach (var externalFaculty in externalFaculties)
        {
            var faculty = await dbContext.Faculties
                .FirstOrDefaultAsync(f => f.Id == externalFaculty.Id, cancellationToken);

            if (faculty == null)
            {
                faculty = new Faculty
                {
                    Id = externalFaculty.Id,
                    Name = externalFaculty.Name,
                    Code = externalFaculty.Id.ToString(), // Use ID as code if not provided
                    Description = null
                };
                dbContext.Faculties.Add(faculty);
                _logger.LogInformation("Added new faculty: {Name} (ID: {Id})", faculty.Name, faculty.Id);
            }
            else
            {
                faculty.Name = externalFaculty.Name;
                _logger.LogDebug("Updated faculty: {Name} (ID: {Id})", faculty.Name, faculty.Id);
            }

            syncedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return syncedCount;
    }

    private async Task<int> SyncPrograms(
        AdmissionDbContext dbContext,
        IExternalDictionaryService externalService,
        CancellationToken cancellationToken)
    {
        var syncedCount = 0;
        var page = 1;
        const int pageSize = 100; // Fetch larger pages for efficiency

        while (true)
        {
            var response = await externalService.GetProgramsAsync(page, pageSize, cancellationToken);

            if (response.Programs == null || !response.Programs.Any())
            {
                break; // No more programs to sync
            }

            foreach (var externalProgram in response.Programs)
            {
                var program = await dbContext.EducationPrograms
                    .FirstOrDefaultAsync(p => p.Id == externalProgram.Id, cancellationToken);

                // Convert education level int ID to Guid
                var educationLevelGuid = ConvertIntToGuid(externalProgram.EducationLevel.Id);

                if (program == null)
                {
                    program = new EducationProgram
                    {
                        Id = externalProgram.Id,
                        Name = externalProgram.Name,
                        Code = externalProgram.Code,
                        EducationLanguage = externalProgram.Language,
                        EducationForm = externalProgram.EducationForm,
                        FacultyId = externalProgram.Faculty.Id,
                        EducationLevelId = educationLevelGuid
                    };
                    dbContext.EducationPrograms.Add(program);
                    _logger.LogInformation("Added new program: {Name} (Code: {Code})", program.Name, program.Code);
                }
                else
                {
                    program.Name = externalProgram.Name;
                    program.Code = externalProgram.Code;
                    program.EducationLanguage = externalProgram.Language;
                    program.EducationForm = externalProgram.EducationForm;
                    program.FacultyId = externalProgram.Faculty.Id;
                    program.EducationLevelId = educationLevelGuid;
                    _logger.LogDebug("Updated program: {Name} (Code: {Code})", program.Name, program.Code);
                }

                syncedCount++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Check if we've reached the last page
            if (response.Pagination.Current >= response.Pagination.Count)
            {
                break;
            }

            page++;
        }

        return syncedCount;
    }
}
