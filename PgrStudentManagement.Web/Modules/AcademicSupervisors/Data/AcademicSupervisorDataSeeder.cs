using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;

public sealed class AcademicSupervisorDataSeeder
{
    private readonly AcademicSupervisorDao _academicSupervisorDao;
    private readonly IStudentLookup _studentLookup;

    public AcademicSupervisorDataSeeder(
        AcademicSupervisorDao academicSupervisorDao,
        IStudentLookup studentLookup)
    {
        _academicSupervisorDao = academicSupervisorDao;
        _studentLookup = studentLookup;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await SeedAcademicSupervisors();

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100001",
            staffNumber: "AS1001",
            role: SupervisoryRole.DirectorOfStudies,
            startDate: new DateTime(2025, 10, 1),
            cancellationToken);

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100001",
            staffNumber: "AS1002",
            role: SupervisoryRole.FirstSupervisor,
            startDate: new DateTime(2025, 10, 1),
            cancellationToken);

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100001",
            staffNumber: "AS1003",
            role: SupervisoryRole.SecondSupervisor,
            startDate: new DateTime(2024, 10, 1),
            cancellationToken,
            endDate: new DateTime(2025, 9, 30));

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100002",
            staffNumber: "AS1001",
            role: SupervisoryRole.FirstSupervisor,
            startDate: new DateTime(2026, 1, 15),
            cancellationToken);

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100002",
            staffNumber: "AS1002",
            role: SupervisoryRole.DirectorOfStudies,
            startDate: new DateTime(2025, 10, 1),
            cancellationToken);

        await SeedAssignmentIfPossibleAsync(
            studentNumber: "S100002",
            staffNumber: "AS1003",
            role: SupervisoryRole.Adviser,
            startDate: new DateTime(2025, 10, 1),
            cancellationToken);
    }

    private async Task SeedAcademicSupervisors()
    {
        await AddAcademicSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "AS1001",
                FirstName = "Robin",
                LastName = "Carter",
                EmailAddress =
                    "robin.carter@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoleEntries =
                [
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1001",
                        Role = SupervisoryRole.DirectorOfStudies
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1001",
                        Role = SupervisoryRole.FirstSupervisor
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1001",
                        Role = SupervisoryRole.SecondSupervisor
                    }
                ]

            });

        await AddAcademicSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "AS1002",
                FirstName = "Jamie",
                LastName = "Ahmed",
                EmailAddress =
                    "jamie.ahmed@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoleEntries =
                [
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1002",
                        Role = SupervisoryRole.DirectorOfStudies
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1002",
                        Role = SupervisoryRole.FirstSupervisor
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1002",
                        Role = SupervisoryRole.SecondSupervisor
                    }
                ]

            });

        await AddAcademicSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "AS1003",
                FirstName = "Taylor",
                LastName = "Green",
                EmailAddress =
                    "taylor.green@example.test",
                EmploymentType =
                    SupervisorEmploymentType.External,
                TrainingCompleted = true,
                EligibleRoleEntries =
                [
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1003",
                        Role = SupervisoryRole.SecondSupervisor
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1003",
                        Role = SupervisoryRole.Adviser
                    }
                ]
            });

        await AddAcademicSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "AS1004",
                FirstName = "Avery",
                LastName = "Brown",
                EmailAddress =
                    "avery.brown@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoleEntries =
                [
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1004",
                        Role = SupervisoryRole.FirstSupervisor
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1004",
                        Role = SupervisoryRole.SecondSupervisor
                    },
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = "AS1004",
                        Role = SupervisoryRole.Adviser
                    }
                ]
            });
    }

    private async Task AddAcademicSupervisorIfMissing(
        AcademicSupervisor supervisor, 
        CancellationToken cancellationToken = default)
    {
        var exists =
            await _academicSupervisorDao
                .StaffNumberExistsAsync(
                    supervisor.StaffNumber,
                    cancellationToken);

        if (!exists)
        {
            await _academicSupervisorDao
                .AddAcademicSupervisorAsync(
                    supervisor,
                    cancellationToken);
        }

    }

    private async Task SeedAssignmentIfPossibleAsync(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        DateTime startDate,
        CancellationToken cancellationToken,
        DateTime? endDate = null)
    {
        startDate = startDate.Date;
        endDate = endDate?.Date;

        StudentSummary? student =
            await _studentLookup.GetStudentSummaryAsync(
                studentNumber,
                cancellationToken);

        if (student is null)
        {
            throw new InvalidOperationException(
                $"Cannot seed supervisory assignment: "
                + $"student {studentNumber} "
                + "does not exist.");
        }

        AcademicSupervisor? supervisor =
            await _academicSupervisorDao.GetAcademicSupervisorAsync(
                staffNumber, cancellationToken);

        if (supervisor is null)
        {
            throw new InvalidOperationException(
                $"Cannot seed supervisory assignment: "
                + $"Academic Supervisor {staffNumber} "
                + "does not exist.");
        }

        var assignments =
            await _academicSupervisorDao
                .GetAssignmentsForStudentAsync(
                    studentNumber,
                    cancellationToken);

        bool assignmentAlreadyExists =
            await _academicSupervisorDao.SupervisoryAssignmentExistsAsync(
                studentNumber, staffNumber, role, startDate, cancellationToken
            );

        if (assignmentAlreadyExists)
        {
            return;
        }

        await _academicSupervisorDao.AddSupervisoryAssignmentAsync(
            new SupervisoryAssignment
            {
                StudentNumber = studentNumber,
                SupervisorStaffNumber = staffNumber,
                Role = role,
                StartDate = startDate,
                EndDate = endDate,
                ExceptionAuthorised = false
            },
            cancellationToken);
    }
}