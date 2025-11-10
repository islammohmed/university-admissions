-- Create databases for the services
CREATE DATABASE IF NOT EXISTS identity_db;
CREATE DATABASE IF NOT EXISTS admission_service;

-- Note: PostgreSQL will use the admission_service database for both AdmissionService and NotificationService
-- as they share the same database in this architecture
