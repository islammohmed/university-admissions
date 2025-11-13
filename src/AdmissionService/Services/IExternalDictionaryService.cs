using AdmissionService.DTOs;

namespace AdmissionService.Services;

public interface IExternalDictionaryService
{
    Task<List<ExternalEducationLevelDto>> GetEducationLevelsAsync(CancellationToken cancellationToken = default);
    Task<List<ExternalDocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);
    Task<List<ExternalFacultyDto>> GetFacultiesAsync(CancellationToken cancellationToken = default);
    Task<ExternalProgramsResponseDto> GetProgramsAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default);
}
