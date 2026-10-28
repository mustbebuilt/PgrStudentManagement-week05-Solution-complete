using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.Students.Data;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;

public class AcademicSupervisorDao
{
    private readonly StudentManagementDbContext _context;

    public AcademicSupervisorDao(StudentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AcademicSupervisor>>
        GetAcademicSupervisorsAsync(
            CancellationToken cancellationToken = default)
    {
        return await _context.AcademicSupervisors
            .AsNoTracking()
            .Include(supervisor => supervisor.EligibleRoleEntries)
            .OrderBy(supervisor => supervisor.LastName)
            .ThenBy(supervisor => supervisor.FirstName)
            .ThenBy(supervisor => supervisor.StaffNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<AcademicSupervisor?>
        GetAcademicSupervisorAsync(
            string staffNumber,
            CancellationToken cancellationToken = default)
    {
        return await _context.AcademicSupervisors
            .AsNoTracking()
            .Include(supervisor => supervisor.EligibleRoleEntries)
            .FirstOrDefaultAsync(
                supervisor =>
                    supervisor.StaffNumber == staffNumber,
                cancellationToken);
    }

    public async Task<bool> StaffNumberExistsAsync(
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.AcademicSupervisors
            .AnyAsync(
                supervisor =>
                    supervisor.StaffNumber == staffNumber,
                cancellationToken);
    }

    public async Task AddAcademicSupervisorAsync(
        AcademicSupervisor supervisor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(supervisor);

        _context.AcademicSupervisors.Add(supervisor);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<SupervisoryAssignment>>
        GetAssignmentsForStudentAsync(
            string studentNumber,
            CancellationToken cancellationToken = default)
    {
        return await _context.SupervisoryAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.StudentNumber == studentNumber)
            .OrderBy(assignment =>
                assignment.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SupervisoryAssignmentExistsAsync(
            string studentNumber,
            string staffNumber,
            SupervisoryRole role,
            DateTime startDate,
            CancellationToken cancellationToken = default)
    {
        return await _context.SupervisoryAssignments
            .AnyAsync(
                assignment =>
                    assignment.StudentNumber == studentNumber
                    && assignment.SupervisorStaffNumber == staffNumber
                    && assignment.Role == role
                    && assignment.StartDate == startDate,
                cancellationToken);
    }

    public async Task AddSupervisoryAssignmentAsync(
        SupervisoryAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        _context.SupervisoryAssignments.Add(assignment);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<bool> UpdateEligibleRolesAsync(
        string staffNumber,
        IEnumerable<SupervisoryRole> roles,
        CancellationToken cancellationToken = default)
    {
        var supervisor = await _context.AcademicSupervisors
            .Include(existing => existing.EligibleRoleEntries)
            .FirstOrDefaultAsync(
                existing => existing.StaffNumber == staffNumber,
                cancellationToken);

        if (supervisor is null)
        {
            return false;
        }

        supervisor.EligibleRoleEntries.Clear();

        foreach (var role in roles.Distinct())
        {
            supervisor.EligibleRoleEntries.Add(
                new AcademicSupervisorEligibleRole
                {
                    SupervisorStaffNumber = staffNumber,
                    Role = role
                });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}