-- Connect to admission_service database
\c admission_service;

-- Create Faculties table
CREATE TABLE IF NOT EXISTS "Faculties" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL
);

-- Create EducationLevels table (Dictionary)
CREATE TABLE IF NOT EXISTS "EducationLevels" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL
);

-- Create EducationPrograms table
CREATE TABLE IF NOT EXISTS "EducationPrograms" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "EducationLanguage" VARCHAR(50),
    "EducationForm" VARCHAR(50),
    "FacultyId" UUID NOT NULL,
    "EducationLevelId" UUID NOT NULL,
    CONSTRAINT "FK_EducationPrograms_Faculties" FOREIGN KEY ("FacultyId") 
        REFERENCES "Faculties"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EducationPrograms_EducationLevels" FOREIGN KEY ("EducationLevelId") 
        REFERENCES "EducationLevels"("Id") ON DELETE RESTRICT
);

-- Create AdmissionPrograms table
CREATE TABLE IF NOT EXISTS "AdmissionPrograms" (
    "Id" UUID PRIMARY KEY,
    "Priority" INTEGER NOT NULL,
    "EducationProgramId" UUID NOT NULL,
    CONSTRAINT "FK_AdmissionPrograms_EducationPrograms" FOREIGN KEY ("EducationProgramId") 
        REFERENCES "EducationPrograms"("Id") ON DELETE CASCADE
);

-- Create Managers table
CREATE TABLE IF NOT EXISTS "Managers" (
    "Id" UUID PRIMARY KEY,
    "FullName" VARCHAR(200) NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "FacultyId" UUID,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Managers_Faculties" FOREIGN KEY ("FacultyId") 
        REFERENCES "Faculties"("Id") ON DELETE SET NULL
);

-- Create Applicants table
CREATE TABLE IF NOT EXISTS "Applicants" (
    "Id" UUID PRIMARY KEY,
    "FullName" VARCHAR(200) NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "BirthDate" TIMESTAMP NOT NULL,
    "Gender" INTEGER NOT NULL,
    "Citizenship" VARCHAR(100),
    "PhoneNumber" VARCHAR(20),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create ApplicantAdmissions table
CREATE TABLE IF NOT EXISTS "ApplicantAdmissions" (
    "Id" UUID PRIMARY KEY,
    "ApplicantId" UUID NOT NULL,
    "ManagerId" UUID,
    "EducationProgramId" UUID NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP,
    CONSTRAINT "FK_ApplicantAdmissions_Applicants" FOREIGN KEY ("ApplicantId") 
        REFERENCES "Applicants"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApplicantAdmissions_Managers" FOREIGN KEY ("ManagerId") 
        REFERENCES "Managers"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_ApplicantAdmissions_EducationPrograms" FOREIGN KEY ("EducationProgramId") 
        REFERENCES "EducationPrograms"("Id") ON DELETE RESTRICT
);

-- Create EducationDocumentTypes table (Dictionary)
CREATE TABLE IF NOT EXISTS "EducationDocumentTypes" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL
);

-- Create Documents table (TPH - Table Per Hierarchy for Document inheritance)
CREATE TABLE IF NOT EXISTS "Documents" (
    "Id" UUID PRIMARY KEY,
    "DocumentType" VARCHAR(50) NOT NULL,
    "ApplicantId" UUID NOT NULL,
    "FileId" UUID NOT NULL,
    "UploadedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    -- Passport fields
    "SeriesNumber" VARCHAR(50),
    "PlaceOfBirth" VARCHAR(200),
    "IssuedDate" TIMESTAMP,
    "IssuedBy" VARCHAR(200),
    -- EducationDocument fields
    "Name" VARCHAR(200),
    "EducationDocumentTypeId" UUID,
    CONSTRAINT "FK_Documents_Applicants" FOREIGN KEY ("ApplicantId") 
        REFERENCES "Applicants"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Documents_EducationDocumentTypes" FOREIGN KEY ("EducationDocumentTypeId") 
        REFERENCES "EducationDocumentTypes"("Id") ON DELETE RESTRICT
);

-- Create Notifications table
CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" UUID PRIMARY KEY,
    "Message" VARCHAR(1000) NOT NULL,
    "UserId" VARCHAR(100) NOT NULL,
    "UserEmail" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "IsSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "SentAt" TIMESTAMP,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "ErrorMessage" VARCHAR(500)
);
