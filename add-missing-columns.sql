-- Run this script in your 'university' database using pgAdmin

-- Add missing columns to Applicants table
ALTER TABLE "Applicants" 
ADD COLUMN IF NOT EXISTS "AppliedAt" TIMESTAMP NOT NULL DEFAULT NOW();

ALTER TABLE "Applicants" 
ADD COLUMN IF NOT EXISTS "Status" INTEGER NOT NULL DEFAULT 0;
