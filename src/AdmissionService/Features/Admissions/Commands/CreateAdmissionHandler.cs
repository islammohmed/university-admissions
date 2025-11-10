using AdmissionService.Data;
using AdmissionService.Entities;
using MediatR;
using Shared.Contracts.Enums;

namespace AdmissionService.Features.Admissions.Commands;

public class CreateAdmissionHandler : IRequestHandler<CreateAdmissionCommand, ApplicantAdmission>
{
    private readonly AdmissionDbContext _context;

    public CreateAdmissionHandler(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicantAdmission> Handle(CreateAdmissionCommand request, CancellationToken cancellationToken)
    {
        var admission = new ApplicantAdmission
        {
            Id = Guid.NewGuid(),
            ApplicantId = request.ApplicantId,
            EducationProgramId = request.EducationProgramId,
            Status = AdmissionStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApplicantAdmissions.Add(admission);
        await _context.SaveChangesAsync(cancellationToken);

        return admission;
    }
}
