using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdmissionClass> Classes => Set<AdmissionClass>();
    public DbSet<SeatHold> Holds => Set<SeatHold>();
    public DbSet<AdminCredential> AdminCredentials => Set<AdminCredential>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AdminCredential>(entity =>
        {
            entity.ToTable("AdminCredentials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        builder.Entity<AdmissionClass>(entity =>
        {
            entity.ToTable("Classes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TotalSeats).IsRequired();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<SeatHold>(entity =>
        {
            entity.ToTable("Holds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.ClassId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.StudentId).IsRequired().HasMaxLength(32);
            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            entity.HasIndex(x => x.ClassId);
            entity.HasIndex(x => x.StudentId);
            entity.HasIndex(x => x.ExpiresAtUtc);

            entity.HasIndex(x => new { x.ClassId, x.StudentId })
                .IsUnique()
                .HasFilter("\"Status\" IN ('HELD', 'CONFIRMED')")
                .HasDatabaseName("IX_Holds_ClassId_StudentId_Active");

            entity.HasOne(x => x.Class)
                .WithMany(x => x.Holds)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
