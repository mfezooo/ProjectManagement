using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.Features.Tasks.Commands.CreateTask;
using ProjectManagement.Application.Features.Tasks.Commands.DeleteTask;
using ProjectManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Application.Features.Tasks.Queries.GetTasksByProject;

namespace ProjectManagement.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lists all tasks for a project.</summary>
    [HttpGet("api/v{version:apiVersion}/projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetByProject(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTasksByProjectQuery { ProjectId = projectId }, cancellationToken);
        return Ok(ApiResponse<List<TaskDto>>.Ok(result));
    }

    /// <summary>Creates a new task inside a project.</summary>
    [HttpPost("api/v{version:apiVersion}/projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create(
        Guid projectId,
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand
        {
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority
        };

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<TaskDto>.Ok(result, "Task created."));
    }

    /// <summary>Updates a task's status.</summary>
    [HttpPatch("api/v{version:apiVersion}/tasks/{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskStatusCommand
        {
            TaskId = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(result, "Status updated."));
    }

    /// <summary>Deletes a task.</summary>
    [HttpDelete("api/v{version:apiVersion}/tasks/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand { TaskId = id }, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { id }, "Task deleted."));
    }
}
