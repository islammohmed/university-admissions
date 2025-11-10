using AdmissionService.Entities;
using MediatR;

namespace AdmissionService.Features.Admissions.Queries;

public record GetAdmissionsByApplicantQuery(Guid ApplicantId) : IRequest<List<ApplicantAdmission>>;
