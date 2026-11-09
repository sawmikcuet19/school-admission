using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SchoolAdmission.Api.Data;

#nullable disable

namespace SchoolAdmission.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261109143609_AddAdminCredentials")]
    partial class AddAdminCredentials
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SchoolAdmission.Api.Domain.AdminCredential", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("AdminCredentials", (string)null);
                });

            modelBuilder.Entity("SchoolAdmission.Api.Domain.AdmissionClass", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("TotalSeats")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Classes", (string)null);
                });

            modelBuilder.Entity("SchoolAdmission.Api.Domain.SeatHold", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("CancelledAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ClassId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ConfirmedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ExpiresAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("StudentId")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("ExpiresAtUtc");

                    b.HasIndex("StudentId");

                    b.HasIndex("ClassId", "StudentId")
                        .IsUnique()
                        .HasDatabaseName("IX_Holds_ClassId_StudentId_Active")
                        .HasFilter("\"Status\" IN ('HELD', 'CONFIRMED')");

                    b.ToTable("Holds", (string)null);
                });

            modelBuilder.Entity("SchoolAdmission.Api.Domain.SeatHold", b =>
                {
                    b.HasOne("SchoolAdmission.Api.Domain.AdmissionClass", "Class")
                        .WithMany("Holds")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");
                });

            modelBuilder.Entity("SchoolAdmission.Api.Domain.AdmissionClass", b =>
                {
                    b.Navigation("Holds");
                });
#pragma warning restore 612, 618
        }
    }
}
