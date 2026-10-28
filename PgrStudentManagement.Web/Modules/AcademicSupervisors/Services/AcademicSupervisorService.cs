using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;

using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

public class AcademicSupervisorService
{
    private readonly AcademicSupervisorDao _academicSupervisorDao;
    private readonly IStudentLookup _studentLookup;

    public AcademicSupervisorService(
        AcademicSupervisorDao academicSupervisorDao,
        IStudentLookup studentLookup)
    {
        _academicSupervisorDao = academicSupervisorDao;
        _studentLookup = studentLookup;
    }

    public async Task<IReadOnlyList<AcademicSupervisor>>
        GetAcademicSupervisorsAsync(
            CancellationToken cancellationToken = default)
    {
        var supervisors = await _academicSupervisorDao
                .GetAcademicSupervisorsAsync(cancellationToken);

        return supervisors
            .OrderBy(supervisor => supervisor.LastName)
            .ThenBy(supervisor => supervisor.FirstName)
            .ToList();
    }

    public Task<AcademicSupervisor?> GetAcademicSupervisorAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        return _academicSupervisorDao.GetAcademicSupervisorAsync(
            staffNumber, cancellationToken);
    }

    public async Task<SupervisorOutcome> CreateAcademicSupervisorAsync(
        AcademicSupervisor supervisor, 
        CancellationToken cancellationToken = default)
    {
        bool staffNumberExists = await _academicSupervisorDao
            .StaffNumberExistsAsync(
                supervisor.StaffNumber,
                cancellationToken);

        if (staffNumberExists)
        {
            return SupervisorOutcome.DuplicateStaffNumber;
        }

        await _academicSupervisorDao.AddAcademicSupervisorAsync(
            supervisor, cancellationToken);

        return SupervisorOutcome.Created;
    }

    public async Task<AssignmentFormResult> PrepareAssignmentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var student =
            await _studentLookup.GetStudentSummaryAsync(
                studentNumber,
                cancellationToken);

        var supervisors = (await _academicSupervisorDao
            .GetAcademicSupervisorsAsync(cancellationToken))
            .OrderBy(supervisor => supervisor.LastName)
            .ThenBy(supervisor => supervisor.FirstName)
            .ThenBy(supervisor => supervisor.StaffNumber)
            .ToList();

        var availableRoles = Enum
            .GetValues<SupervisoryRole>()
            .ToList();

        return new AssignmentFormResult
        {
            Student = student,
            Supervisors = supervisors,
            AvailableRoles = availableRoles
        };
    }

    public async Task<AssignmentOutcome> AssignSupervisorAsync(
        AssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student =
            await _studentLookup.GetStudentSummaryAsync(
                request.StudentNumber,
                cancellationToken);

        if (student is null)
        {
            return AssignmentOutcome.StudentNotFound;
        }

        var supervisor = await
            _academicSupervisorDao.GetAcademicSupervisorAsync(
                request.StaffNumber, cancellationToken);

        if (supervisor is null)
        {
            return AssignmentOutcome.SupervisorNotFound;
        }

        var assignments =
            await _academicSupervisorDao
                .GetAssignmentsForStudentAsync(
                    request.StudentNumber,
                    cancellationToken);

        var currentAssignments = assignments
            .Where(assignment =>
                assignment.Current)
            .ToList();

        var duplicateAssignment =
            currentAssignments.Any(assignment =>
                assignment.SupervisorStaffNumber
                    == request.StaffNumber
                && assignment.Role == request.Role);

        if (duplicateAssignment)
        {
            return AssignmentOutcome.DuplicateAssignment;
        }

        var roleMustBeUnique =
            request.Role
                == SupervisoryRole.DirectorOfStudies
            || request.Role
                == SupervisoryRole.FirstSupervisor;

        if (roleMustBeUnique)
        {
            var roleAlreadyFilled =
                currentAssignments.Any(assignment =>
                    assignment.Role == request.Role);

            if (roleAlreadyFilled)
            {
                return AssignmentOutcome.RoleAlreadyFilled;
            }
        }

        bool supervisorIsEligible = supervisor.IsEligibleFor(request.Role);

        if (!supervisorIsEligible)
        {
            return AssignmentOutcome.SupervisorNotEligible;
        }

        var requiresInstitutionEmployee =
            request.Role
                == SupervisoryRole.DirectorOfStudies
            || request.Role
                == SupervisoryRole.FirstSupervisor;

        var hasPermittedEmploymentType =
            supervisor.EmploymentType
                == SupervisorEmploymentType
                    .InstitutionEmployed;

        if (requiresInstitutionEmployee
            && !hasPermittedEmploymentType)
        {
            return AssignmentOutcome
                .EmploymentTypeNotPermitted;
        }

        var currentSupervisorCount =
            currentAssignments.Count(assignment =>
                assignment.Role != SupervisoryRole.Adviser);

        var addingFourthSupervisor = currentSupervisorCount >= 3;

        if (addingFourthSupervisor
            && !request.ExceptionAuthorised)
        {
            return AssignmentOutcome.ExceptionRequired;
        }

        var assignment = new SupervisoryAssignment
        {
            StudentNumber = request.StudentNumber,
            SupervisorStaffNumber =
                request.StaffNumber,
            Role = request.Role,
            StartDate = request.StartDate,
            EndDate = null,
            ExceptionAuthorised =
                request.ExceptionAuthorised
        };

        await _academicSupervisorDao.AddSupervisoryAssignmentAsync(
            assignment, cancellationToken);

        return AssignmentOutcome.Created;
    }

    public async Task<SupervisoryTeamResult> GetSupervisoryTeamAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var student =
            await  _studentLookup.GetStudentSummaryAsync(
                studentNumber,
                cancellationToken);

        if (student is null)
        {
            return new SupervisoryTeamResult
            {
                StudentFound = false
            };
        }

        var assignments =
            await _academicSupervisorDao
                .GetAssignmentsForStudentAsync(
                    studentNumber,
                    cancellationToken);

        List<SupervisoryAssignmentViewModel> resolvedAssignments = 
            new List<SupervisoryAssignmentViewModel>();

        foreach (SupervisoryAssignment assignment in assignments)
        {
            SupervisoryAssignmentViewModel? viewModel = await
                CreateAssignmentViewModelAsync(
                    assignment, cancellationToken
                );

            if (viewModel is not null)
            {
                resolvedAssignments.Add(viewModel);
            }
        }

        var currentAssignments = resolvedAssignments
            .Where(assignment => assignment.Current)
            .OrderBy(assignment => assignment.Role)
            .ThenBy(assignment =>
                assignment.SupervisorName)
            .ToList();

        var historicalAssignments =
            resolvedAssignments
                .Where(assignment =>
                    !assignment.Current)
                .OrderByDescending(assignment =>
                    assignment.EndDate)
                .ThenBy(assignment =>
                    assignment.Role)
                .ToList();

        return new SupervisoryTeamResult
        {
            StudentFound = true,
            Student = student,
            CurrentAssignments = currentAssignments,
            HistoricalAssignments = historicalAssignments
        };
    }

    private async Task<SupervisoryAssignmentViewModel?>
        CreateAssignmentViewModelAsync(
            SupervisoryAssignment assignment,
            CancellationToken cancellationToken)
    {
        var supervisor =
            await _academicSupervisorDao
                .GetAcademicSupervisorAsync(
                    assignment.SupervisorStaffNumber,
                    cancellationToken);

        if (supervisor is null)
        {
            return null;
        }

        return new SupervisoryAssignmentViewModel
        {
            SupervisorStaffNumber = supervisor.StaffNumber,
            SupervisorName = $"{supervisor.FirstName} " + supervisor.LastName,
            EmploymentType = supervisor.EmploymentType,
            Role = assignment.Role,
            StartDate = assignment.StartDate,
            EndDate = assignment.EndDate
        };
    }

}