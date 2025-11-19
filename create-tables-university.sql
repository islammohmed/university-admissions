-- Run this script in your 'university' database using pgAdmin or any PostgreSQL client

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

-- Create Identity tables (AspNetUsers, AspNetRoles, etc.)
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" VARCHAR(450) PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" VARCHAR(450) PRIMARY KEY,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,
    "TwoFactorEnabled" BOOLEAN NOT NULL,
    "LockoutEnd" TIMESTAMP WITH TIME ZONE,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    "FullName" VARCHAR(200) NOT NULL,
    "Role" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") 
        REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" VARCHAR(450) NOT NULL,
    "ProviderKey" VARCHAR(450) NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" VARCHAR(450) NOT NULL,
    "LoginProvider" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(450) NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") 
        REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles"("RoleId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims"("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins"("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims"("RoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles"("NormalizedName");
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers"("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers"("NormalizedUserName");

-- Insert seed data for Education Levels
INSERT INTO "EducationLevels" ("Id", "Name") VALUES 
('11111111-1111-1111-1111-111111111111', 'Bachelor'),
('22222222-2222-2222-2222-222222222222', 'Master'),
('33333333-3333-3333-3333-333333333333', 'PhD')
ON CONFLICT DO NOTHING;

-- Insert seed data for Education Document Types
INSERT INTO "EducationDocumentTypes" ("Id", "Name") VALUES 
('d1111111-1111-1111-1111-111111111111', 'High School Diploma'),
('d2222222-2222-2222-2222-222222222222', 'Bachelor Diploma'),
('d3333333-3333-3333-3333-333333333333', 'Master Diploma')
ON CONFLICT DO NOTHING;

-- Insert seed data for Faculties
INSERT INTO "Faculties" ("Id", "Name") VALUES 
('f1111111-1111-1111-1111-111111111111', 'Faculty of Computer Science'),
('f2222222-2222-2222-2222-222222222222', 'Faculty of Engineering'),
('f3333333-3333-3333-3333-333333333333', 'Faculty of Business Administration')
ON CONFLICT DO NOTHING;

-- Insert seed data for Education Programs
INSERT INTO "EducationPrograms" ("Id", "Name", "Code", "EducationLanguage", "EducationForm", "FacultyId", "EducationLevelId") VALUES 
('e1111111-1111-1111-1111-111111111111', 'Computer Science', 'CS-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111'),
('e2222222-2222-2222-2222-222222222222', 'Software Engineering', 'SE-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111'),
('e3333333-3333-3333-3333-333333333333', 'Data Science', 'DS-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222'),
('e4444444-4444-4444-4444-444444444444', 'Mechanical Engineering', 'ME-001', 'English', 'Full-time', 'f2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111'),
('e5555555-5555-5555-5555-555555555555', 'Business Administration', 'BA-001', 'English', 'Full-time', 'f3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111')
ON CONFLICT DO NOTHING;
