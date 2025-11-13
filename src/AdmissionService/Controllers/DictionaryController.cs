using AdmissionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace AdmissionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DictionaryController : ControllerBase
{
    private readonly IExternalDictionaryService _externalService;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<DictionaryController> _logger;

    public DictionaryController(
        IExternalDictionaryService externalService,
        ISchedulerFactory schedulerFactory,
        ILogger<DictionaryController> logger)
    {
        _externalService = externalService;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get education levels from external API
    /// </summary>
    [HttpGet("education-levels")]
    public async Task<IActionResult> GetEducationLevels(CancellationToken cancellationToken)
    {
        try
        {
            var levels = await _externalService.GetEducationLevelsAsync(cancellationToken);
            return Ok(levels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching education levels");
            return StatusCode(500, new { error = "Failed to fetch education levels" });
        }
    }

    /// <summary>
    /// Get document types from external API
    /// </summary>
    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _externalService.GetDocumentTypesAsync(cancellationToken);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document types");
            return StatusCode(500, new { error = "Failed to fetch document types" });
        }
    }

    /// <summary>
    /// Get faculties from external API
    /// </summary>
    [HttpGet("faculties")]
    public async Task<IActionResult> GetFaculties(CancellationToken cancellationToken)
    {
        try
        {
            var faculties = await _externalService.GetFacultiesAsync(cancellationToken);
            return Ok(faculties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching faculties");
            return StatusCode(500, new { error = "Failed to fetch faculties" });
        }
    }

    /// <summary>
    /// Get programs from external API with pagination
    /// </summary>
    [HttpGet("programs")]
    public async Task<IActionResult> GetPrograms(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var programs = await _externalService.GetProgramsAsync(page, size, cancellationToken);
            return Ok(programs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching programs");
            return StatusCode(500, new { error = "Failed to fetch programs" });
        }
    }

    /// <summary>
    /// Manually trigger external data synchronization (Admin only)
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> TriggerSync()
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey("ExternalDataSyncJob");
            
            // Trigger the job immediately
            await scheduler.TriggerJob(jobKey);
            
            _logger.LogInformation("External data sync triggered manually");
            return Ok(new { message = "Synchronization started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering sync");
            return StatusCode(500, new { error = "Failed to trigger synchronization" });
        }
    }
}
