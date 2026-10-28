namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

public class SupervisoryAssignment
{
    public string StudentNumber { get; set; } = string.Empty;

    public string SupervisorStaffNumber { get; set; } = string.Empty;

    public SupervisoryRole Role { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool ExceptionAuthorised { get; set; }

    public bool Current => EndDate is null;
}