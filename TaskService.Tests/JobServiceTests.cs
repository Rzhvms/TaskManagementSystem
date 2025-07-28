using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskService.Controllers.DTO.Requests;
using TaskService.Data.Entities;
using TaskService.Data.Repositories.Interfaces;
using TaskService.Logic.Services;
using TaskService.Logic.Services.Interfaces;

namespace TaskService.Tests;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _repoMock;
    private readonly Mock<IJobHistoryRepository> _historyRepoMock;
    private readonly Mock<ILogger<JobService>> _loggerMock;
    private readonly Mock<INotificationClient> _notificationClientMock;
    private readonly JobService _service;

    public JobServiceTests()
    {
        _repoMock = new Mock<IJobRepository>();
        _historyRepoMock = new Mock<IJobHistoryRepository>();
        _loggerMock = new Mock<ILogger<JobService>>();
        _notificationClientMock = new Mock<INotificationClient>();

        _service = new JobService(
            _repoMock.Object,
            _historyRepoMock.Object,
            _loggerMock.Object,
            _notificationClientMock.Object);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldCreateJobAndLogHistory_ReturnsJobId()
    {
        var request = new CreateJobRequest
        {
            Name = "Test Job", 
            Description = "Desc", 
            Deadline = DateTime.UtcNow.AddDays(1)
        };
        int userId = 1;
        int expectedJobId = 42;

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Job>())).ReturnsAsync(expectedJobId);
        _historyRepoMock.Setup(h => h.AddAsync(It.IsAny<JobHistory>())).Returns(Task.CompletedTask);

        var jobId = await _service.CreateJobAsync(request, userId);

        jobId.Should().Be(expectedJobId);
        _repoMock.Verify(r => r.CreateAsync(It.Is<Job>(j =>
            j.Name == request.Name &&
            j.Description == request.Description &&
            j.CreatedBy == userId &&
            j.Status == JobStatus.ToDo &&
            !j.IsDeleted)), Times.Once);

        _historyRepoMock.Verify(h => h.AddAsync(It.Is<JobHistory>(history =>
            history.JobId == expectedJobId &&
            history.ChangedBy == userId &&
            history.Action == JobActionType.Created)), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()
                    .Contains("Задача создана") || v.ToString().Contains("JobId")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task GetJobByIdAsync_WhenJobExists_ReturnsMappedResponse()
    {
        int jobId = 10;
        var job = new Job
        {
            Id = jobId,
            Name = "Name",
            Description = "Desc",
            Status = JobStatus.ToDo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 2,
            IsDeleted = false
        };

        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);

        var response = await _service.GetJobByIdAsync(jobId);

        response.Should().NotBeNull();
        response!.Id.Should().Be(job.Id);
        response.Name.Should().Be(job.Name);
    }

    [Fact]
    public async Task GetJobByIdAsync_WhenJobIsDeleted_ReturnsNull()
    {
        int jobId = 10;
        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(new Job { IsDeleted = true });

        var response = await _service.GetJobByIdAsync(jobId);

        response.Should().BeNull();
    }

    [Fact]
    public async Task GetFilteredJobsAsync_ReturnsMappedJobs()
    {
        var filter = new JobFilterRequest { };
        var jobs = new List<Job>
        {
            new() { Id = 1, Name = "abc1", IsDeleted = false },
            new() { Id = 2, Name = "abc2", IsDeleted = false }
        };

        _repoMock.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(jobs);
        
        var results = await _service.GetFilteredJobsAsync(filter);

        results.Should().HaveCount(2);
        results.Select(j => j.Id).Should().Contain(new[] {1, 2});
    }

    [Fact]
    public async Task UpdateJobAsync_WhenJobNotFound_ReturnsFalse()
    {
        int jobId = 1;
        int userId = 2;
        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync((Job?)null);

        var updateRequest = new UpdateJobRequest
        {
            Name = "NewName",
            Description = "NewDesc",
            Deadline = DateTime.UtcNow,
            Status = JobStatus.Done
        };

        var result = await _service.UpdateJobAsync(jobId, updateRequest, userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateJobAsync_WhenUserIsNotCreator_ReturnsFalse()
    {
        int jobId = 1;
        int userId = 2;
        var job = new Job { Id = jobId, CreatedBy = 99, IsDeleted = false };
        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);

        var updateRequest = new UpdateJobRequest
        {
            Name = "NewName",
            Description = "NewDesc",
            Deadline = DateTime.UtcNow,
            Status = JobStatus.Done
        };

        var result = await _service.UpdateJobAsync(jobId, updateRequest, userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateJobAsync_WhenUpdateSucceeds_ReturnsTrueAndLogsHistory()
    {
        int jobId = 1;
        int userId = 1;
        var job = new Job
        {
            Id = jobId,
            CreatedBy = userId,
            Name = "OldName",
            Description = "OldDesc",
            Status = JobStatus.ToDo,
            IsDeleted = false
        };

        var updateRequest = new UpdateJobRequest
        {
            Name = "NewName",
            Description = "NewDesc",
            Deadline = DateTime.UtcNow,
            Status = JobStatus.Done
        };

        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Job>())).ReturnsAsync(true);
        _historyRepoMock.Setup(h => h.AddAsync(It.IsAny<JobHistory>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateJobAsync(jobId, updateRequest, userId);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Job>(j => j.Name == updateRequest.Name)), Times.Once);
        _historyRepoMock.Verify(h => h.AddAsync(It.Is<JobHistory>(h => h.Action == JobActionType.Updated)), Times.Once);
    }

    [Fact]
    public async Task DeleteJobAsync_WhenJobExists_ReturnsTrueAndLogsHistory()
    {
        int jobId = 1;
        int userId = 2;
        var job = new Job { Id = jobId, IsDeleted = false };

        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.DeleteAsync(jobId)).ReturnsAsync(true);
        _historyRepoMock.Setup(h => h.AddAsync(It.IsAny<JobHistory>())).Returns(Task.CompletedTask);

        var result = await _service.DeleteJobAsync(jobId, userId);
        
        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(jobId), Times.Once);
        _historyRepoMock.Verify(h => h.AddAsync(It.Is<JobHistory>(h => h.Action == JobActionType.Deleted)), Times.Once);
    }

    [Fact]
    public async Task AssignPerformerAsync_WhenJobExists_ReturnsTrue_AndSendsNotification()
    {
        int jobId = 1;
        int performerId = 5;
        int userId = 10;
        var job = new Job { Id = jobId, Name = "JobName", Deadline = DateTime.UtcNow.AddDays(3), IsDeleted = false };

        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.AssignPerformerAsync(jobId, performerId)).ReturnsAsync(true);
        _historyRepoMock.Setup(h => h.AddAsync(It.IsAny<JobHistory>())).Returns(Task.CompletedTask);
        _notificationClientMock.Setup(n => n.SendNotificationAsync(It.IsAny<CreateNotificationRequest>()))
            .Returns(Task.CompletedTask);

        var result = await _service.AssignPerformerAsync(jobId, performerId, userId);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.AssignPerformerAsync(jobId, performerId), Times.Once);
        _historyRepoMock.Verify(h => h.AddAsync(It.Is<JobHistory>(h => h.Action == JobActionType.Assigned)), Times.Once);
        _notificationClientMock.Verify(n => n.SendNotificationAsync(It.Is<CreateNotificationRequest>(r =>
            r.UserId == performerId && r.Title.Contains("назначена новая задача") &&
            r.Message.Contains(job.Name))), Times.Once);
    }

    [Fact]
    public async Task AssignPerformerAsync_WhenJobNotFound_ReturnsFalse()
    {
        int jobId = 1;
        int performerId = 5;
        int userId = 10;

        _repoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync((Job?)null);

        var result = await _service.AssignPerformerAsync(jobId, performerId, userId);

        result.Should().BeFalse();
    }
}