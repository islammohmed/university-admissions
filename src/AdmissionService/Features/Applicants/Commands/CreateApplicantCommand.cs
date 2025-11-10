using AdmissionService.Entities;
using MediatR;
using Shared.Contracts.Enums;

namespace AdmissionService.Features.Applicants.Commands;

public record CreateApplicantCommand(
    string FullName,
    string Email,
    DateTime BirthDate,
    Gender Gender,
    string Citizenship,
    string PhoneNumber
) : IRequest<Applicant>;
