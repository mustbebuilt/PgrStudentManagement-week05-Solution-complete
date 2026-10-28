using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data;

public class StudentManagementDbContext : DbContext
{
    public StudentManagementDbContext(
        DbContextOptions<StudentManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();

    // Tables for academic supervisor module
    public DbSet<AcademicSupervisor> AcademicSupervisors =>
        Set<AcademicSupervisor>();

    public DbSet<AcademicSupervisorEligibleRole>
        AcademicSupervisorEligibleRoles =>
            Set<AcademicSupervisorEligibleRole>();

    public DbSet<SupervisoryAssignment> SupervisoryAssignments =>
        Set<SupervisoryAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureStudents(modelBuilder);
        ConfigureAcademicSupervisors(modelBuilder);

    }

    private static void ConfigureStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(student =>
                student.StudentNumber);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(enrolment => enrolment.EnrolmentId);

            entity
            .HasOne(enrolment => enrolment.Student)
            .WithMany(student => student.Enrolments)
            .HasForeignKey(enrolment => enrolment.StudentNumber)
            .OnDelete(DeleteBehavior.Restrict);

            entity
            .Property(enrolment => enrolment.Status)
            .HasConversion<string>()
            .HasMaxLength(30);
        });
    }

    private static void ConfigureAcademicSupervisors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicSupervisor>(
            entity =>
            {
                entity.ToTable("AcademicSupervisors");
                entity.HasKey(supervisor => supervisor.StaffNumber);
                entity.Property(supervisor => supervisor.StaffNumber)
                    .IsRequired();

                entity.Property(supervisor => supervisor.FirstName);
                entity.Property(supervisor => supervisor.LastName);

                entity.Property(supervisor => supervisor.EmailAddress)
                    .IsRequired();

                entity.HasMany(supervisor => supervisor.EligibleRoleEntries)
                    .WithOne(eligibleRole =>
                        eligibleRole.AcademicSupervisor)
                    .HasForeignKey(eligibleRole =>
                        eligibleRole.SupervisorStaffNumber)
                    .OnDelete(DeleteBehavior.Cascade);

            });

        modelBuilder.Entity<AcademicSupervisorEligibleRole>(
            entity =>
            {
                entity.ToTable("AcademicSupervisorEligibleRoles");

                entity.HasKey(eligibleRole => new
                {
                    eligibleRole.SupervisorStaffNumber,
                    eligibleRole.Role
                });

                entity.Property(eligibleRole => eligibleRole.Role)
                    .HasConversion<string>()
                    .IsRequired();
            });

        modelBuilder.Entity<SupervisoryAssignment>(
            entity =>
            {
                entity.ToTable("SupervisoryAssignments");

                entity.HasKey(assignment => new
                {
                    assignment.StudentNumber,
                    assignment.SupervisorStaffNumber,
                    assignment.Role,
                    assignment.StartDate
                });

                entity.Property(assignment => assignment.Role)
                    .HasConversion<string>();

                entity.HasOne<Student>()
                    .WithMany()
                    .HasForeignKey(assignment => assignment.StudentNumber)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<AcademicSupervisor>()
                    .WithMany()
                    .HasForeignKey(assignment =>
                        assignment.SupervisorStaffNumber)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(assignment => assignment.Current);
            });
    }
}
