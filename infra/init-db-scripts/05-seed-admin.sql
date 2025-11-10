-- Connect to identity_db database
\c identity_db;

-- Note: This script is optional as Identity tables are created by EF Core migrations
-- However, you can add seed data for admin users here if needed

-- Sample: Insert a head manager user (password would be hashed by Identity)
-- This would typically be done through the application's registration endpoint
-- or via EF Core seeding in the IdentityService

-- Example structure (actual implementation should use Identity APIs):
-- INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", ...)
-- VALUES (...);

-- For now, this file serves as a placeholder for future admin user seeding
