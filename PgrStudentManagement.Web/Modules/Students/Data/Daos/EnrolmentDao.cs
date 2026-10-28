using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data.Daos;

public class EnrolmentDao
{
    private readonly StudentManagementDbContext _context;

    public EnrolmentDao(StudentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Enrolment>> GetEnrolmentsForStudentAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Enrolments
            .AsNoTracking()
            .Where(enrolment => 
                enrolment.StudentNumber == studentNumber)
            .ToListAsync();
    }

    public async Task<bool> EnrolmentExistsAsync(
        int enrolmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Enrolments
            .AnyAsync(
                enrolment =>
                    enrolment.EnrolmentId
                        == enrolmentId,
                cancellationToken);
    }

    public async Task AddEnrolmentAsync(
        Enrolment enrolment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(enrolment);

        _context.Enrolments.Add(enrolment);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

}