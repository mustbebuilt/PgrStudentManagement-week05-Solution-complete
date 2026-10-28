namespace PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

public class SupervisoryAssignmentViewModel
{
    public string SupervisorStaffNumber { get; init; }
        = string.Empty;

    public string SupervisorName { get; init; }
        = string.Empty;

    public SupervisorEmploymentType EmploymentType
    {
        get;
        init;
    }

    public SupervisoryRole Role { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public bool Current => EndDate is null;
}