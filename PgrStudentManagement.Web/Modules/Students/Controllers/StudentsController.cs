using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Modules.Students.Models;
using PgrStudentManagement.Web.Modules.Students.Services;

namespace PgrStudentManagement.Web.Modules.Students.Controllers;

public sealed class StudentsController : Controller
{
    private readonly StudentService _studentService;

    public StudentsController(StudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var students = await _studentService.GetStudentsAsync(
            cancellationToken);

        return View(students);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Student());
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Student student,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(student);
        }

        var result = await _studentService.CreateStudentAsync(
            student,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                nameof(Student.StudentNumber),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(Details),
            new { studentNumber = student.StudentNumber });
    }

    public async Task<IActionResult> Details(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    public async Task<IActionResult> Enrolments(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var enrolments = await _studentService
            .GetEnrolmentsForStudentAsync(
                studentNumber,
                cancellationToken);

        ViewData["StudentNumber"] = studentNumber;

        if (enrolments.Count == 0)
        {
            ViewData["Message"] = "No enrolments found";
        }

        return View(enrolments);
    }

    public async Task<IActionResult> EnrolmentDetails(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    public async Task<IActionResult> ThesisDetails(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        if (string.IsNullOrWhiteSpace(student.ThesisTitle))
        {
            ViewData["Message"] =
                "Thesis information not available";
        }

        return View(student);
    }

    [HttpGet]
    public async Task<IActionResult> EditStatus(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    public async Task<IActionResult> EditStatus(
        string studentNumber,
        Status status,
        CancellationToken cancellationToken)
    {
        var result = await _studentService.UpdateStatusAsync(
            studentNumber,
            status,
            cancellationToken);

        if (!result.Succeeded)
        {
            return View("StudentNotFound");
        }

        return RedirectToAction(
            nameof(Details),
            new { studentNumber });
    }

    [HttpGet]
    public async Task<IActionResult> EditExpectedSubmissionDate(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    public async Task<IActionResult> EditExpectedSubmissionDate(
        string studentNumber,
        DateTime? expectedSubmissionDate,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService
                .UpdateExpectedSubmissionDateAsync(
                    studentNumber,
                    expectedSubmissionDate,
                    cancellationToken);

        if (!result.Succeeded)
        {
            var student =
                await _studentService.GetStudentAsync(
                    studentNumber,
                    cancellationToken);

            if (student is null)
            {
                return View("StudentNotFound");
            }

            ModelState.AddModelError(
                nameof(Student.ExpectedSubmissionDate),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(ThesisDetails),
            new { studentNumber });
    }

    [HttpGet]
    public async Task<IActionResult> RecordSubmission(
        string studentNumber,
        CancellationToken cancellationToken)
    {
        var student = await _studentService.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordSubmission(
        string studentNumber,
        DateTime? actualSubmissionDate,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService.RecordSubmissionAsync(
                studentNumber,
                actualSubmissionDate,
                cancellationToken);

        if (!result.Succeeded)
        {
            var student =
                await _studentService.GetStudentAsync(
                    studentNumber,
                    cancellationToken);

            if (student is null)
            {
                return View("StudentNotFound");
            }

            ModelState.AddModelError(
                nameof(Student.ActualSubmissionDate),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(ThesisDetails),
            new { studentNumber });
    }
}