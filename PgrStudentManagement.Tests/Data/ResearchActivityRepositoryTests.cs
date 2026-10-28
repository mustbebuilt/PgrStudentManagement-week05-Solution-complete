using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using PgrStudentManagement.Web.Modules.ResearchActivities.Data;
using PgrStudentManagement.Web.Modules.ResearchActivities.Models;
using PgrStudentManagement.Web.Modules.ResearchActivities.Services;
using Xunit;

namespace PgrStudentManagement.Tests.Data;

public class ResearchActivityRepositoryTests : IDisposable
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly string _collectionName;
    private readonly ResearchActivityRepository _repository;
    private readonly ResearchActivityService _service;

    public ResearchActivityRepositoryTests()
    {
        var connectionString = "mongodb://localhost:27017";
        _client = new MongoClient(connectionString);
        var databaseName = "StudentManagementTest";
        _database = _client.GetDatabase(databaseName);
        _collectionName = "ResearchActivities_" + Guid.NewGuid().ToString("N");

        var configValues = new Dictionary<string, string?>
        {
            ["ResearchActivityDatabase:ConnectionString"] = connectionString,
            ["ResearchActivityDatabase:DatabaseName"] = databaseName,
            ["ResearchActivityDatabase:CollectionName"] = _collectionName
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _repository = new ResearchActivityRepository(_client, configuration);
        _service = new ResearchActivityService(_repository);
    }

    public void Dispose()
    {
        _database.DropCollection(_collectionName);
    }

    [Fact]
    public async Task DefaultDatabase_ContainsSeededActivities()
    {
        var defaultDb = _client.GetDatabase("StudentManagement");
        var collection = defaultDb.GetCollection<ResearchActivity>("ResearchActivities");
        var activities = await collection.Find(_ => true).ToListAsync();
        Assert.True(activities.Count >= 3);
        Assert.Contains(activities, a => a.Id == "activity-001" && a.StudentNumber == "S100001");
        Assert.Contains(activities, a => a.Id == "activity-002" && a.StudentNumber == "S100001");
        Assert.Contains(activities, a => a.Id == "activity-003" && a.StudentNumber == "S100002");
    }

    [Fact]
    public async Task SeedAsync_AddsExpectedActivities()
    {
        await ResearchActivityDataSeeder.SeedAsync(_repository);

        var s1Activities = await _service.GetActivitiesForStudentAsync("S100001");
        Assert.Equal(2, s1Activities.Count);

        var s2Activities = await _service.GetActivitiesForStudentAsync("S100002");
        Assert.Single(s2Activities);

        var s4Activities = await _service.GetActivitiesForStudentAsync("S100004");
        Assert.Empty(s4Activities);
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicate()
    {
        await ResearchActivityDataSeeder.SeedAsync(_repository);
        await ResearchActivityDataSeeder.SeedAsync(_repository);

        var s1Activities = await _service.GetActivitiesForStudentAsync("S100001");
        Assert.Equal(2, s1Activities.Count);
    }

    [Fact]
    public async Task GetActivitiesForStudentAsync_ReturnsChronologicalOrder()
    {
        await ResearchActivityDataSeeder.SeedAsync(_repository);

        var activities = await _service.GetActivitiesForStudentAsync("S100001");
        Assert.Equal(2, activities.Count);
        Assert.True(activities[0].ActivityDate <= activities[1].ActivityDate);
        Assert.Equal(ResearchActivityType.ResearchReflection, activities[0].ActivityType);
        Assert.Equal(ResearchActivityType.SupervisionMeeting, activities[1].ActivityType);
    }

    [Fact]
    public async Task AddAsync_StoresCustomDetailsProperly()
    {
        var activity = new ResearchActivity
        {
            Id = "activity-test-01",
            StudentNumber = "S100003",
            ActivityType = ResearchActivityType.Experiment,
            Title = "Lab Experiment 1",
            ActivityDate = new DateTime(2026, 4, 1),
            CreatedOn = DateTime.UtcNow,
            Tags = ["experiment", "data"],
            Details = new Dictionary<string, object>
            {
                ["hypothesis"] = "Testing hypothesis A",
                ["sampleSize"] = 50
            }
        };

        await _repository.AddAsync(activity);

        var retrieved = await _service.GetActivitiesForStudentAsync("S100003");
        var single = Assert.Single(retrieved);
        Assert.Equal("activity-test-01", single.Id);
        Assert.Equal("Lab Experiment 1", single.Title);
        Assert.Equal("Testing hypothesis A", single.Details["hypothesis"].ToString());
    }
}
