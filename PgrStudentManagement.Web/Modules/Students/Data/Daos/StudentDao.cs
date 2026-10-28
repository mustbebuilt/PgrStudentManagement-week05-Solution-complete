using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data.Daos;

public class StudentDao
{
    private readonly StudentManagementDbContext _context;

    public StudentDao(StudentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .OrderBy(student => student.StudentNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(
                student => student.StudentNumber == studentNumber,
                cancellationToken);
    }

    public async Task<bool> StudentNumberExistsAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AnyAsync(
                student => student.StudentNumber == studentNumber,
                cancellationToken);
    }

    public async Task AddStudentAsync(
        Student student,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);

        _context.Students.Add(student);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateStudentAsync(
        Student student,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);

        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(
                existing =>
                    existing.StudentNumber == student.StudentNumber,
                cancellationToken);

        if (existingStudent is null)
        {
            return false;
        }

        _context.Entry(existingStudent)
            .CurrentValues
            .SetValues(student);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}