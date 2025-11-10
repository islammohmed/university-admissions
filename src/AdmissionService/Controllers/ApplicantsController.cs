using AdmissionService.Features.Applicants.Commands;
using AdmissionService.Features.Applicants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdmissionService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateApplicant([FromBody] CreateApplicantCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetApplicant), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplicant(Guid id)
    {
        var result = await _mediator.Send(new GetApplicantByIdQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
