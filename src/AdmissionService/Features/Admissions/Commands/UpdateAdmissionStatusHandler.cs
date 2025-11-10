using AdmissionService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdmissionService.Features.Admissions.Commands;

public class UpdateAdmissionStatusHandler : IRequestHandler<UpdateAdmissionStatusCommand, bool>
{
    private readonly AdmissionDbContext _context;

    public UpdateAdmissionStatusHandler(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAdmissionStatusCommand request, CancellationToken cancellationToken)
    {
        var admission = await _context.ApplicantAdmissions
            .FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);

        if (admission == null)
            return false;

        admission.Status = request.NewStatus;
        admission.UpdatedAt = DateTime.UtcNow;
        
        if (request.ManagerId.HasValue)
        {
            admission.ManagerId = request.ManagerId.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
