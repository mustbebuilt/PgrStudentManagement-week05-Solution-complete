using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

using PgrStudentManagement.Web.Modules.Students.Contracts;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Services;

using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

using PgrStudentManagement.Web.Modules.ResearchActivities.Data;
using PgrStudentManagement.Web.Modules.ResearchActivities.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services.
builder.Services.AddControllersWithViews();

// Configure relational persistence.
builder.Services.AddDbContext<StudentManagementDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "StudentManagementDatabase")));

// Configure MongoDB persistence.
builder.Services.AddSingleton<IMongoClient>(
    _ => new MongoClient(
        builder.Configuration[
            "ResearchActivityDatabase:ConnectionString"]));

// Students module.
builder.Services.AddScoped<StudentDao>();
builder.Services.AddScoped<EnrolmentDao>();
builder.Services.AddScoped<StudentService>();

// Register StudentService as the implementation
// of the IStudentLookup contract.
builder.Services.AddScoped<IStudentLookup>(
    provider =>
        provider.GetRequiredService<StudentService>());

// Students-module seeders.
builder.Services.AddScoped<StudentDataSeeder>();
builder.Services.AddScoped<EnrolmentDataSeeder>();

// Research Activities module.
builder.Services.AddScoped<ResearchActivityRepository>();
builder.Services.AddScoped<ResearchActivityService>();

// Academic Supervisors module.
builder.Services.AddScoped<AcademicSupervisorDao>();
builder.Services.AddScoped<AcademicSupervisorService>();
builder.Services.AddScoped<AcademicSupervisorDataSeeder>();

var app = builder.Build();

// Apply EF Core migrations and seed initial data.
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context =
        serviceProvider.GetRequiredService<
            StudentManagementDbContext>();

    await context.Database.MigrateAsync();

    // Students must be seeded before Enrolments
    // because Enrolments refer to StudentNumber.
    var studentSeeder =
        serviceProvider.GetRequiredService<
            StudentDataSeeder>();

    await studentSeeder.SeedAsync();

    var enrolmentSeeder =
        serviceProvider.GetRequiredService<
            EnrolmentDataSeeder>();

    await enrolmentSeeder.SeedAsync();

    var academicSupervisorSeeder =
        serviceProvider.GetRequiredService<
            AcademicSupervisorDataSeeder>();

    await academicSupervisorSeeder.SeedAsync();

    var researchActivityRepository =
        serviceProvider.GetRequiredService<
            ResearchActivityRepository>();

    await ResearchActivityDataSeeder.SeedAsync(
        researchActivityRepository);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// .NET 10 static asset endpoints.
app.MapStaticAssets();

// Research Activities route.
app.MapControllerRoute(
        name: "research_activities",
        pattern:
            "ResearchActivities/{action=Index}/{id?}",
        defaults: new
        {
            controller = "ResearchActivity"
        })
    .WithStaticAssets();

// Default MVC route.
app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

