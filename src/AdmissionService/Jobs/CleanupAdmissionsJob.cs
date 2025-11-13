using AdmissionService.Data;
using AdmissionService.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Shared.Contracts.Enums;

namespace AdmissionService.Jobs;

public class CleanupAdmissionsJob : IJob
{
    private readonly ILogger<CleanupAdmissionsJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupAdmissionsJob(
        ILogger<CleanupAdmissionsJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting CleanupAdmissionsJob at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();

        try
        {
            // Find applicants that have been under review for more than 3 days
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
            
            var applicantsToClose = await dbContext.Applicants
                .Where(a => a.Status == AdmissionStatus.UnderReview && 
                           a.AppliedAt < threeDaysAgo)
                .ToListAsync();

            if (applicantsToClose.Any())
            {
                foreach (var applicant in applicantsToClose)
                {
                    applicant.Status = AdmissionStatus.Closed;
                    _logger.LogInformation(
                        "Closing applicant {ApplicantId} ({Email}) - under review since {AppliedAt}",
                        applicant.Id, applicant.Email, applicant.AppliedAt);
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "CleanupAdmissionsJob completed. Closed {Count} applications", 
                    applicantsToClose.Count);
            }
            else
            {
                _logger.LogInformation("CleanupAdmissionsJob completed. No applications to close");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while running CleanupAdmissionsJob");
            throw;
        }
    }
}
