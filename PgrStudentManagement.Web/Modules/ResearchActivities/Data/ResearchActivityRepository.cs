using MongoDB.Driver;
using PgrStudentManagement.Web.Modules.ResearchActivities.Models;

namespace PgrStudentManagement.Web.Modules.ResearchActivities.Data;

public class ResearchActivityRepository
{
    private readonly IMongoCollection<ResearchActivity> _activities;

    public ResearchActivityRepository(
        IMongoClient client,
        IConfiguration configuration)
    {
        var databaseName = configuration["ResearchActivityDatabase:DatabaseName"]!;
        var collectionName = configuration["ResearchActivityDatabase:CollectionName"]!;
        var database = client.GetDatabase(databaseName);
        _activities = database.GetCollection<ResearchActivity>(collectionName);
    }

    public async Task<IReadOnlyList<ResearchActivity>> GetForStudentAsync(string studentNumber)
    {
        return await _activities
            .Find(activity => activity.StudentNumber == studentNumber)
            .SortBy(activity => activity.ActivityDate)
            .ToListAsync();
    }

    public Task AddAsync(ResearchActivity activity)
    {
        return _activities.InsertOneAsync(activity);
    }
}
