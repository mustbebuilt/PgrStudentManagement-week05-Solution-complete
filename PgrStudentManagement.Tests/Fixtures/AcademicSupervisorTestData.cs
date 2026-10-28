using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

namespace PgrStudentManagement.Tests.Fixtures;

public static class AcademicSupervisorTestData
{
    public static AcademicSupervisor CreateInstitutionSupervisor(
            string staffNumber = "SUP001",
            params SupervisoryRole[] eligibleRoles)
    {
        var roles = eligibleRoles.Length == 0
            ?
            [
                SupervisoryRole.DirectorOfStudies,
                SupervisoryRole.FirstSupervisor,
                SupervisoryRole.SecondSupervisor
            ]
            : eligibleRoles;

        return new AcademicSupervisor
        {
            StaffNumber = staffNumber,
            FirstName = "Robin",
            LastName = "Carter",
            EmailAddress =
                $"{staffNumber.ToLowerInvariant()}" +
                "@example.test",

            EmploymentType =
                SupervisorEmploymentType
                    .InstitutionEmployed,

            TrainingCompleted = true,

            EligibleRoleEntries = roles
                .Distinct()
                .Select(role =>
                    new AcademicSupervisorEligibleRole
                    {
                        SupervisorStaffNumber = staffNumber,
                        Role = role
                    })
                .ToList()
        };
    }

    public static AcademicSupervisor CreateExternalSupervisor(
            string staffNumber = "EXT001",
            params SupervisoryRole[] eligibleRoles)
    {
        var supervisor = CreateInstitutionSupervisor(
            staffNumber,
            eligibleRoles);

        supervisor.EmploymentType = SupervisorEmploymentType.External;

        return supervisor;
    }
}