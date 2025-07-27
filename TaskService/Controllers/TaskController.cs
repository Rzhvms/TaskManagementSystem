using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Controllers.DTO.Requests;
using TaskService.Controllers.DTO.Responses;
using TaskService.Logic.Services.Interfaces;

namespace TaskService.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly IJobService _jobService;
    
    public TaskController(IJobService jobService)
    {
        _jobService = jobService;
    }

    private int GetUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdStr, out var userId))
            throw new Exception("UserID is not valid");

        return userId;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetJobResponse>>> GetTasks([FromQuery] JobFilterRequest filter)
    {
        var jobs = await _jobService.GetFilteredJobsAsync(filter);
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetJobResponse>> GetTaskById(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        return job is null ? NotFound() : Ok(job);
    }
    
    [HttpPost]
    public async Task<ActionResult<int>> CreateTask([FromBody] CreateJobRequest request)
    {
        var userId = GetUserId();
        var jobId = await _jobService.CreateJobAsync(request, userId);
        return CreatedAtAction(nameof(GetTaskById), new { id = jobId }, jobId);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateJobRequest request)
    {
        var userId = GetUserId();
        var updated = await _jobService.UpdateJobAsync(id, request, userId);
        return updated ? Ok("Задача изменена.") : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        var deleted = await _jobService.DeleteJobAsync(id, userId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{id}/assign")]
    public async Task<ActionResult> AssignPerformer(int id, [FromBody] AssignPerformerRequest request)
    {
        var userId = GetUserId();
        var assigned = await _jobService.AssignPerformerAsync(id, request.PerformerId, userId);
        return assigned ? Ok("Исполнитель назначен.") : NotFound();
    }
}