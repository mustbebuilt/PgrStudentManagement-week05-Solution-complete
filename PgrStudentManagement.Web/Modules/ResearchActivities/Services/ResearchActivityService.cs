using PgrStudentManagement.Web.Modules.ResearchActivities.Data;
using PgrStudentManagement.Web.Modules.ResearchActivities.Models;

namespace PgrStudentManagement.Web.Modules.ResearchActivities.Services;

public class ResearchActivityService
{
    private readonly ResearchActivityRepository _repository;

    public ResearchActivityService(ResearchActivityRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ResearchActivity>> GetActivitiesForStudentAsync(string studentNumber)
    {
        return _repository.GetForStudentAsync(studentNumber);
    }
}
