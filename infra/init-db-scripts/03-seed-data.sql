-- Connect to admission_service database
\c admission_service;

-- Seed EducationLevels (Dictionary table)
INSERT INTO "EducationLevels" ("Id", "Name") VALUES
    ('11111111-1111-1111-1111-111111111111', 'Bachelor'),
    ('22222222-2222-2222-2222-222222222222', 'Master'),
    ('33333333-3333-3333-3333-333333333333', 'PhD')
ON CONFLICT ("Id") DO NOTHING;

-- Seed EducationDocumentTypes (Dictionary table)
INSERT INTO "EducationDocumentTypes" ("Id", "Name") VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'High School Diploma'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Bachelor Diploma'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Master Diploma')
ON CONFLICT ("Id") DO NOTHING;

-- Seed sample Faculties
INSERT INTO "Faculties" ("Id", "Name") VALUES
    ('f1111111-1111-1111-1111-111111111111', 'Faculty of Computer Science'),
    ('f2222222-2222-2222-2222-222222222222', 'Faculty of Engineering'),
    ('f3333333-3333-3333-3333-333333333333', 'Faculty of Business Administration')
ON CONFLICT ("Id") DO NOTHING;

-- Seed sample EducationPrograms
INSERT INTO "EducationPrograms" ("Id", "Name", "Code", "EducationLanguage", "EducationForm", "FacultyId", "EducationLevelId") VALUES
    ('e1111111-1111-1111-1111-111111111111', 'Computer Science', 'CS-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111'),
    ('e2222222-2222-2222-2222-222222222222', 'Software Engineering', 'SE-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111'),
    ('e3333333-3333-3333-3333-333333333333', 'Data Science', 'DS-001', 'English', 'Full-time', 'f1111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222'),
    ('e4444444-4444-4444-4444-444444444444', 'Mechanical Engineering', 'ME-001', 'English', 'Full-time', 'f2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111'),
    ('e5555555-5555-5555-5555-555555555555', 'Business Administration', 'BA-001', 'English', 'Full-time', 'f3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111')
ON CONFLICT ("Id") DO NOTHING;
