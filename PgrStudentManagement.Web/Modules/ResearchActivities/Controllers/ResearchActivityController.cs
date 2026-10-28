using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Modules.ResearchActivities.Services;

namespace PgrStudentManagement.Web.Modules.ResearchActivities.Controllers;

public class ResearchActivityController : Controller
{
    private readonly ResearchActivityService _service;

    public ResearchActivityController(ResearchActivityService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string studentNumber)
    {
        var activities = await _service.GetActivitiesForStudentAsync(studentNumber);

        ViewData["StudentNumber"] = studentNumber;

        if (activities.Count == 0)
        {
            ViewData["Message"] = "No research activities found";
        }

        return View("~/Views/ResearchActivities/Index.cshtml", activities);
    }
}

public class ResearchActivitiesController : ResearchActivityController
{
    public ResearchActivitiesController(ResearchActivityService service) : base(service)
    {
    }
}
