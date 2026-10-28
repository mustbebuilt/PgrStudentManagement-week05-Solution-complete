using System.ComponentModel.DataAnnotations.Schema;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

public class AcademicSupervisor
{
    public string StaffNumber { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public string EmailAddress { get; set; } = string.Empty;

    public SupervisorEmploymentType EmploymentType { get; set; }

    public bool TrainingCompleted { get; set; }

    public ICollection<AcademicSupervisorEligibleRole> 
        EligibleRoleEntries { get; set; } =
            new List<AcademicSupervisorEligibleRole>();

    public bool IsEligibleFor(SupervisoryRole role)
    {
        return EligibleRoleEntries.Any(
            eligibleRole => eligibleRole.Role == role);
    }

    [NotMapped]
    public IReadOnlySet<SupervisoryRole> EligibleRoles =>
        EligibleRoleEntries
            .Select(entry => entry.Role)
            .ToHashSet();


}