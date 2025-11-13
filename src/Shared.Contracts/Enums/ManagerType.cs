namespace Shared.Contracts.Enums;

/// <summary>
/// Type of manager in the admission system
/// </summary>
public enum ManagerType
{
    /// <summary>
    /// Faculty Manager - works in the admissions campaign for a specific faculty
    /// and is responsible for applicants applying to programs within that faculty
    /// </summary>
    FacultyManager = 0,
    
    /// <summary>
    /// Head Manager - manager of the main admission campaign who, in addition to
    /// working with applicants, monitors the overall progress of the campaign
    /// </summary>
    HeadManager = 1
}
