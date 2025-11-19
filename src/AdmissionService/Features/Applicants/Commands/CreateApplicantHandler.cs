using AdmissionService.Data;
using AdmissionService.Entities;
using MediatR;

namespace AdmissionService.Features.Applicants.Commands;

public class CreateApplicantHandler : IRequestHandler<CreateApplicantCommand, Applicant>
{
    private readonly AdmissionDbContext _context;

    public CreateApplicantHandler(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<Applicant> Handle(CreateApplicantCommand request, CancellationToken cancellationToken)
    {
        var applicant = new Applicant
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            Citizenship = request.Citizenship,
            PhoneNumber = request.PhoneNumber,
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync(cancellationToken);

        return applicant;
    }
}
