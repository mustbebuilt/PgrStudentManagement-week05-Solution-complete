using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

public class AssignmentFormResult
{
    public StudentSummary? Student { get; init; }

    public IReadOnlyList<AcademicSupervisor>
        Supervisors { get; init; }
        = [];

    public IReadOnlyList<SupervisoryRole>
        AvailableRoles { get; init; }
        = [];
}