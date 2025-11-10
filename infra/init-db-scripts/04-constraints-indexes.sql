-- Connect to admission_service database
\c admission_service;

-- Create indexes for performance optimization
CREATE INDEX IF NOT EXISTS "IX_Applicants_Email" ON "Applicants"("Email");
CREATE INDEX IF NOT EXISTS "IX_ApplicantAdmissions_ApplicantId_EducationProgramId" 
    ON "ApplicantAdmissions"("ApplicantId", "EducationProgramId");
CREATE INDEX IF NOT EXISTS "IX_ApplicantAdmissions_Status" ON "ApplicantAdmissions"("Status");
CREATE INDEX IF NOT EXISTS "IX_Notifications_IsSent" ON "Notifications"("IsSent");
CREATE INDEX IF NOT EXISTS "IX_Notifications_CreatedAt" ON "Notifications"("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Documents_ApplicantId" ON "Documents"("ApplicantId");
CREATE INDEX IF NOT EXISTS "IX_EducationPrograms_FacultyId" ON "EducationPrograms"("FacultyId");
CREATE INDEX IF NOT EXISTS "IX_EducationPrograms_EducationLevelId" ON "EducationPrograms"("EducationLevelId");
CREATE INDEX IF NOT EXISTS "IX_Managers_FacultyId" ON "Managers"("FacultyId");

-- Add constraints (if not already added in table creation)
-- These are declarative and help maintain data integrity

-- Ensure email uniqueness for applicants
CREATE UNIQUE INDEX IF NOT EXISTS "UIX_Applicants_Email" ON "Applicants"("Email");

-- Ensure education program codes are unique
CREATE UNIQUE INDEX IF NOT EXISTS "UIX_EducationPrograms_Code" ON "EducationPrograms"("Code");
