using System.Text.Json.Serialization;

namespace AdmissionService.DTOs;

// Education Levels - /api/dictionary/education_levels
public class ExternalEducationLevelDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// Document Types - /api/dictionary/document_types
public class ExternalDocumentTypeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("createTime")]
    public DateTime CreateTime { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("educationLevel")]
    public ExternalEducationLevelDto EducationLevel { get; set; } = null!;
    
    [JsonPropertyName("nextEducationLevels")]
    public List<ExternalEducationLevelDto> NextEducationLevels { get; set; } = new();
}

// Faculties - /api/dictionary/faculties
public class ExternalFacultyDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("createTime")]
    public DateTime CreateTime { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// Programs - /api/dictionary/programs
public class ExternalProgramDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("createTime")]
    public DateTime CreateTime { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
    
    [JsonPropertyName("educationForm")]
    public string EducationForm { get; set; } = string.Empty;
    
    [JsonPropertyName("faculty")]
    public ExternalFacultyDto Faculty { get; set; } = null!;
    
    [JsonPropertyName("educationLevel")]
    public ExternalEducationLevelDto EducationLevel { get; set; } = null!;
}

public class ExternalProgramsResponseDto
{
    [JsonPropertyName("programs")]
    public List<ExternalProgramDto> Programs { get; set; } = new();
    
    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; } = null!;
}

public class PaginationDto
{
    [JsonPropertyName("size")]
    public int Size { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("current")]
    public int Current { get; set; }
}
