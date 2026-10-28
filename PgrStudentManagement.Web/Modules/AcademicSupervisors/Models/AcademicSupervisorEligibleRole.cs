namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

public sealed class AcademicSupervisorEligibleRole
{
    public string SupervisorStaffNumber { get; set; } = string.Empty;

    public SupervisoryRole Role { get; set; }

    public AcademicSupervisor AcademicSupervisor
    {
        get;
        set;
    } = null!;
}
