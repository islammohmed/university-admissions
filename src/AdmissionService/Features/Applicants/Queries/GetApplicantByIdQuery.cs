using AdmissionService.Entities;
using MediatR;

namespace AdmissionService.Features.Applicants.Queries;

public record GetApplicantByIdQuery(Guid Id) : IRequest<Applicant?>;
