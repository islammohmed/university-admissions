using AdmissionService.Features.Admissions.Commands;
using AdmissionService.Features.Admissions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdmissionService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmission([FromBody] CreateAdmissionCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAdmissionsByApplicant), new { applicantId = result.ApplicantId }, result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "FacultyManager,HeadManager")]
    public async Task<IActionResult> UpdateAdmissionStatus(Guid id, [FromBody] UpdateAdmissionStatusCommand command)
    {
        if (id != command.AdmissionId)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("applicant/{applicantId}")]
    public async Task<IActionResult> GetAdmissionsByApplicant(Guid applicantId)
    {
        var result = await _mediator.Send(new GetAdmissionsByApplicantQuery(applicantId));
        return Ok(result);
    }
}
