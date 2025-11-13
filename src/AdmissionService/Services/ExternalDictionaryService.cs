using AdmissionService.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdmissionService.Services;

public class ExternalDictionaryService : IExternalDictionaryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalDictionaryService> _logger;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;

    public ExternalDictionaryService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ExternalDictionaryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        
        _baseUrl = _configuration["ExternalApi:BaseUrl"] ?? "https://1c-mockup.kreosoft.space";
        _username = _configuration["ExternalApi:Username"] ?? "student";
        _password = _configuration["ExternalApi:Password"] ?? "ny6gQnyn4ecbBrP9l1Fz";
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_baseUrl);
        
        // Add Basic Authentication header
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        
        return client;
    }

    public async Task<List<ExternalEducationLevelDto>> GetEducationLevelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/dictionary/education_levels", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch education levels. Status: {StatusCode}", response.StatusCode);
                return new List<ExternalEducationLevelDto>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var levels = JsonSerializer.Deserialize<List<ExternalEducationLevelDto>>(content, options);
            return levels ?? new List<ExternalEducationLevelDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching education levels from external API");
            throw;
        }
    }

    public async Task<List<ExternalDocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/dictionary/document_types", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch document types. Status: {StatusCode}", response.StatusCode);
                return new List<ExternalDocumentTypeDto>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var types = JsonSerializer.Deserialize<List<ExternalDocumentTypeDto>>(content, options);
            return types ?? new List<ExternalDocumentTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document types from external API");
            throw;
        }
    }

    public async Task<List<ExternalFacultyDto>> GetFacultiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/dictionary/faculties", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch faculties. Status: {StatusCode}", response.StatusCode);
                return new List<ExternalFacultyDto>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var faculties = JsonSerializer.Deserialize<List<ExternalFacultyDto>>(content, options);
            return faculties ?? new List<ExternalFacultyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching faculties from external API");
            throw;
        }
    }

    public async Task<ExternalProgramsResponseDto> GetProgramsAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync($"/api/dictionary/programs?page={page}&size={size}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch programs. Status: {StatusCode}", response.StatusCode);
                return new ExternalProgramsResponseDto();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var programsResponse = JsonSerializer.Deserialize<ExternalProgramsResponseDto>(content, options);
            return programsResponse ?? new ExternalProgramsResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching programs from external API");
            throw;
        }
    }
}
