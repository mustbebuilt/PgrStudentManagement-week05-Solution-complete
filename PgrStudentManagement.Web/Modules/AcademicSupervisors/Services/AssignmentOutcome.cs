namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

public enum AssignmentOutcome
{
    Created,
    StudentNotFound,
    SupervisorNotFound,
    DuplicateAssignment,
    RoleAlreadyFilled,
    SupervisorNotEligible,
    EmploymentTypeNotPermitted,
    ExceptionRequired
}