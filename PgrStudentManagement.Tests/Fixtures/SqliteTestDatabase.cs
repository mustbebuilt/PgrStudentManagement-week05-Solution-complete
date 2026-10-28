using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Web.Modules.Students.Data;

namespace PgrStudentManagement.Tests.Fixtures;

public sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public StudentManagementDbContext Context { get; }

    private SqliteTestDatabase(
        SqliteConnection connection,
        StudentManagementDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<StudentManagementDbContext>()
                .UseSqlite(connection)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .Options;

        var context = new StudentManagementDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return new SqliteTestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}