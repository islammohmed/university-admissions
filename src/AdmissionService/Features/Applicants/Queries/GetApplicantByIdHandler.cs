using AdmissionService.Data;
using AdmissionService.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdmissionService.Features.Applicants.Queries;

public class GetApplicantByIdHandler : IRequestHandler<GetApplicantByIdQuery, Applicant?>
{
    private readonly AdmissionDbContext _context;

    public GetApplicantByIdHandler(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<Applicant?> Handle(GetApplicantByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Applicants
            .Include(a => a.ApplicantAdmissions)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
    }
}
