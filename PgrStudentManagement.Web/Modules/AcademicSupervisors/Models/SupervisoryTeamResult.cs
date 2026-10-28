using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

public class SupervisoryTeamResult
{
    public bool StudentFound { get; init; }
    
    public StudentSummary? Student { get; init; }

    public IReadOnlyList<SupervisoryAssignmentViewModel>
        CurrentAssignments { get; init; }
        = [];

    public IReadOnlyList<SupervisoryAssignmentViewModel>
        HistoricalAssignments { get; init; }
        = [];
}