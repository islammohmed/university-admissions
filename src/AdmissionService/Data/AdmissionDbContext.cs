using AdmissionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdmissionService.Data;

public class AdmissionDbContext : DbContext
{
    public AdmissionDbContext(DbContextOptions<AdmissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<EducationLevel> EducationLevels { get; set; }
    public DbSet<EducationProgram> EducationPrograms { get; set; }
    public DbSet<AdmissionProgram> AdmissionPrograms { get; set; }
    public DbSet<ApplicantAdmission> ApplicantAdmissions { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Passport> Passports { get; set; }
    public DbSet<EducationDocument> EducationDocuments { get; set; }
    public DbSet<EducationDocumentType> EducationDocumentTypes { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applicant configuration
        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Citizenship).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.Email);
        });

        // Manager configuration
        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Faculty)
                .WithMany(f => f.Managers)
                .HasForeignKey(e => e.FacultyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Faculty configuration
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        });

        // EducationLevel configuration
        modelBuilder.Entity<EducationLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        // EducationProgram configuration
        modelBuilder.Entity<EducationProgram>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EducationLanguage).HasMaxLength(50);
            entity.Property(e => e.EducationForm).HasMaxLength(50);
            
            entity.HasOne(e => e.Faculty)
                .WithMany(f => f.EducationPrograms)
                .HasForeignKey(e => e.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EducationLevel)
                .WithMany(el => el.EducationPrograms)
                .HasForeignKey(e => e.EducationLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AdmissionProgram configuration
        modelBuilder.Entity<AdmissionProgram>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Priority).IsRequired();
            
            entity.HasOne(e => e.EducationProgram)
                .WithMany(ep => ep.AdmissionPrograms)
                .HasForeignKey(e => e.EducationProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApplicantAdmission configuration
        modelBuilder.Entity<ApplicantAdmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            
            entity.HasOne(e => e.Applicant)
                .WithMany(a => a.ApplicantAdmissions)
                .HasForeignKey(e => e.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Manager)
                .WithMany(m => m.ApplicantAdmissions)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.EducationProgram)
                .WithMany(ep => ep.ApplicantAdmissions)
                .HasForeignKey(e => e.EducationProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ApplicantId, e.EducationProgramId });
        });

        // Document configuration (TPH - Table Per Hierarchy)
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            
            entity.HasDiscriminator<string>("DocumentType")
                .HasValue<Passport>("Passport")
                .HasValue<EducationDocument>("EducationDocument");

            entity.HasOne(e => e.Applicant)
                .WithMany(a => a.Documents)
                .HasForeignKey(e => e.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Passport configuration
        modelBuilder.Entity<Passport>(entity =>
        {
            entity.Property(e => e.SeriesNumber).HasMaxLength(50);
            entity.Property(e => e.PlaceOfBirth).HasMaxLength(200);
            entity.Property(e => e.IssuedBy).HasMaxLength(200);
        });

        // EducationDocumentType configuration
        modelBuilder.Entity<EducationDocumentType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        // EducationDocument configuration
        modelBuilder.Entity<EducationDocument>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            
            entity.HasOne(e => e.EducationDocumentType)
                .WithMany(edt => edt.EducationDocuments)
                .HasForeignKey(e => e.EducationDocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.IsSent);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
