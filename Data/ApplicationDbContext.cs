using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Midix.Models;

namespace Midix.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentStateChange> AppointmentStateChanges => Set<AppointmentStateChange>();
        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
        public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<Medicine> Medicines => Set<Medicine>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser 
            builder.Entity<ApplicationUser>(e =>
            {
                e.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                e.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                e.Property(u => u.Gender).HasConversion<string>();
                e.Property(u => u.Role).HasConversion<string>();
            });

            // Patient
            builder.Entity<Patient>(e =>
            {
                e.HasKey(p => p.UserId);
                e.HasOne(p => p.User)
                 .WithOne(u => u.Patient)
                 .HasForeignKey<Patient>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(p => p.BloodType).HasMaxLength(10);
            });

            // Doctor
            builder.Entity<Doctor>(e =>
            {
                e.HasKey(d => d.UserId);
                e.HasOne(d => d.User)
                 .WithOne(u => u.Doctor)
                 .HasForeignKey<Doctor>(d => d.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(d => d.Specialization)
                 .WithMany(s => s.Doctors)
                 .HasForeignKey(d => d.SpecializationId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.Property(d => d.ConsultationFee).HasColumnType("decimal(10,2)");
                e.Property(d => d.FollowUpFee).HasColumnType("decimal(10,2)");
                e.Property(d => d.Bio).HasMaxLength(1000);
                e.Property(d => d.ClinicAddress).HasMaxLength(500);
            });

            // WorkingHours 
            builder.Entity<WorkingHours>(e =>
            {
                e.HasOne(w => w.Doctor)
                 .WithMany(d => d.WorkingHours)
                 .HasForeignKey(w => w.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(w => w.Day).IsRequired();
            });

            // Rating 
            builder.Entity<Rating>(e =>
            {
                e.HasOne(r => r.Patient)
                 .WithMany(p => p.Ratings)
                 .HasForeignKey(r => r.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Doctor)
                 .WithMany(d => d.Ratings)
                 .HasForeignKey(r => r.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(r => r.Rate).IsRequired();
                e.Property(r => r.Comment).HasMaxLength(1000);
            });

            // Appointment 
            builder.Entity<Appointment>(e =>
            {
                e.HasOne(a => a.Patient)
                 .WithMany(p => p.Appointments)
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(a => a.Doctor)
                 .WithMany(d => d.Appointments)
                 .HasForeignKey(a => a.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.Property(a => a.State)
                 .IsRequired()
                 .HasConversion<string>()
                 .HasMaxLength(50);
                e.HasIndex(a => new { a.DoctorId, a.Date }).IsUnique();


            });

            // AppointmentStateChange
            builder.Entity<AppointmentStateChange>(e =>
            {
                e.HasOne(sc => sc.Appointment)
                 .WithMany(a => a.StateChanges)
                 .HasForeignKey(sc => sc.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(sc => sc.OldState).HasConversion<string>().HasMaxLength(50);
                e.Property(sc => sc.NewState).HasConversion<string>().HasMaxLength(50);
            });

            // MedicalRecord
            builder.Entity<MedicalRecord>(e =>
            {
                e.HasOne(mr => mr.Appointment)
                 .WithOne(a => a.MedicalRecord)
                 .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(mr => mr.Diagnosis).HasMaxLength(2000);
            });

            // Prescription
            builder.Entity<Prescription>(e =>
            {
                e.HasOne(pr => pr.MedicalRecord)
                 .WithMany(mr => mr.Prescriptions)
                 .HasForeignKey(pr => pr.MedicalRecordId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(pr => pr.Patient)
                 .WithMany()
                 .HasForeignKey(pr => pr.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                // ── Column constraints ───────────────────────────────────────────
                e.Property(pr => pr.Notes)
                 .HasMaxLength(2000);

                e.Property(pr => pr.ImageUrl)
                 .IsRequired()
                 .HasMaxLength(1000);

                // AiAnalysisResult stores raw JSON; map to nvarchar(max) / TEXT.
                e.Property(pr => pr.AiAnalysisResult)
                 .IsRequired(false)
                 .HasColumnType("nvarchar(max)");

                e.Property(pr => pr.UploadedAt)
                 .IsRequired()
                 .HasDefaultValueSql("GETUTCDATE()");

            });

            builder.Entity<Medicine>(e =>
            {
                e.HasOne(m => m.Prescription)
                 .WithMany(pr => pr.Medicines)
                 .HasForeignKey(m => m.PrescriptionId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(m => m.Dosage).HasMaxLength(100);
                e.Property(m => m.Frequency).HasMaxLength(100);
                e.Property(m => m.Duration).HasMaxLength(100);
            });

            builder.Entity<MedicalDocument>(e =>
            {
                e.HasOne(md => md.MedicalRecord)
                 .WithMany(mr => mr.MedicalDocuments)
                 .HasForeignKey(md => md.MedicalRecordId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(md => md.FileName).HasMaxLength(255);
                e.Property(md => md.FilePath).HasMaxLength(500);
            });

            builder.Entity<Payment>(e =>
            {
                e.HasOne(p => p.Appointment)
                 .WithOne(a => a.Payment)
                 .HasForeignKey<Payment>(p => p.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
                e.Property(p => p.Tax).HasColumnType("decimal(10,2)");
                e.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
                e.Property(p => p.Method).HasConversion<string>().HasMaxLength(50);
            });
        }
    }
}