using AdmissionService.Data;
using AdmissionService.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdmissionService.Features.Admissions.Queries;

public class GetAdmissionsByApplicantHandler : IRequestHandler<GetAdmissionsByApplicantQuery, List<ApplicantAdmission>>
{
    private readonly AdmissionDbContext _context;

    public GetAdmissionsByApplicantHandler(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicantAdmission>> Handle(GetAdmissionsByApplicantQuery request, CancellationToken cancellationToken)
    {
        return await _context.ApplicantAdmissions
            .Include(a => a.EducationProgram)
                .ThenInclude(ep => ep.Faculty)
            .Include(a => a.EducationProgram)
                .ThenInclude(ep => ep.EducationLevel)
            .Include(a => a.Manager)
            .Where(a => a.ApplicantId == request.ApplicantId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
