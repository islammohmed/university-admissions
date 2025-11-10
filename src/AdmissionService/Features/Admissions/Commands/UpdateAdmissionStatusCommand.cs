using MediatR;
using Shared.Contracts.Enums;

namespace AdmissionService.Features.Admissions.Commands;

public record UpdateAdmissionStatusCommand(
    Guid AdmissionId,
    AdmissionStatus NewStatus,
    Guid? ManagerId
) : IRequest<bool>;
