using AdmissionService.Entities;
using MediatR;
using Shared.Contracts.Enums;

namespace AdmissionService.Features.Admissions.Commands;

public record CreateAdmissionCommand(
    Guid ApplicantId,
    Guid EducationProgramId
) : IRequest<ApplicantAdmission>;
