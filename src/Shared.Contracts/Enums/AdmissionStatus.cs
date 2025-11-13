namespace Shared.Contracts.Enums;

/// <summary>
/// Status of an admission application.
/// Workflow: Created → UnderReview → Confirmed/Rejected → Closed
/// </summary>
public enum AdmissionStatus
{
    /// <summary>
    /// The applicant has started the admission process and is entering data
    /// </summary>
    Created = 0,
    
    /// <summary>
    /// The manager has added the admission to their tracked list for data verification
    /// </summary>
    UnderReview = 1,
    
    /// <summary>
    /// The data is correct; the manager has confirmed it
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// The manager has checked the data and found it incomplete or incorrect
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Editing access is closed. Once this status is set, the applicant can no longer make any changes
    /// </summary>
    Closed = 4
}
