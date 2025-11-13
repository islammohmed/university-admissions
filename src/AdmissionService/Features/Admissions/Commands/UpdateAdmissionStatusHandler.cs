using AdmissionService.Data;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace AdmissionService.Features.Admissions.Commands;

public class UpdateAdmissionStatusHandler : IRequestHandler<UpdateAdmissionStatusCommand, bool>
{
    private readonly AdmissionDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateAdmissionStatusHandler> _logger;

    public UpdateAdmissionStatusHandler(
        AdmissionDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<UpdateAdmissionStatusHandler> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateAdmissionStatusCommand request, CancellationToken cancellationToken)
    {
        var admission = await _context.ApplicantAdmissions
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);

        if (admission == null)
            return false;

        var oldStatus = admission.Status.ToString();
        admission.Status = request.NewStatus;
        admission.UpdatedAt = DateTime.UtcNow;
        
        if (request.ManagerId.HasValue)
        {
            admission.ManagerId = request.ManagerId.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish event to notify applicant of status change
        try
        {
            await _publishEndpoint.Publish(new ApplicantStatusChangedEvent
            {
                ApplicantId = admission.ApplicantId,
                OldStatus = oldStatus,
                NewStatus = admission.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                ApplicantEmail = admission.Applicant?.Email ?? string.Empty,
                ApplicantName = admission.Applicant?.FullName ?? string.Empty
            }, cancellationToken);

            _logger.LogInformation(
                "Published ApplicantStatusChangedEvent for Admission {AdmissionId}", 
                admission.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish ApplicantStatusChangedEvent for Admission {AdmissionId}", 
                admission.Id);
            // Don't fail the operation if event publishing fails
        }

        return true;
    }
}
