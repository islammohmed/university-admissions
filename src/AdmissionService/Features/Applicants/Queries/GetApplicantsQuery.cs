using AdmissionService.Data;
using AdmissionService.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Enums;

namespace AdmissionService.Features.Applicants.Queries;

public record GetApplicantsQuery(
    AdmissionStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<GetApplicantsResult>;

public record GetApplicantsResult(
    List<ApplicantDto> Applicants,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record ApplicantDto(
    Guid Id,
    string FullName,
    string Email,
    AdmissionStatus Status,
    DateTime AppliedAt
);

public class GetApplicantsQueryHandler : IRequestHandler<GetApplicantsQuery, GetApplicantsResult>
{
    private readonly AdmissionDbContext _context;
    private readonly ILogger<GetApplicantsQueryHandler> _logger;

    public GetApplicantsQueryHandler(
        AdmissionDbContext context,
        ILogger<GetApplicantsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetApplicantsResult> Handle(GetApplicantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching applicants - Page: {Page}, Size: {Size}, Status: {Status}", 
            request.PageNumber, request.PageSize, request.Status);

        var query = _context.Applicants.AsQueryable();

        // Filter by status if provided
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var applicants = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ApplicantDto(
                a.Id,
                a.FullName,
                a.Email,
                a.Status,
                a.AppliedAt
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} applicants out of {Total}", 
            applicants.Count, totalCount);

        return new GetApplicantsResult(
            applicants,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
