using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.Features.Projects.Commands.CreateProject;
using ProjectManagement.Application.Features.Projects.Commands.DeleteProject;
using ProjectManagement.Application.Features.Projects.Commands.UpdateProject;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Queries.GetAllProjects;
using ProjectManagement.Application.Features.Projects.Queries.GetProjectById;

namespace ProjectManagement.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all projects visible to the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllProjectsQuery(), cancellationToken);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    /// <summary>Returns a project by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery { Id = id }, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    /// <summary>Creates a new project owned by the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id, version = "1.0" },
            ApiResponse<ProjectDto>.Ok(result, "Project created."));
    }

    /// <summary>Updates an existing project.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, "Project updated."));
    }

    /// <summary>Deletes a project.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProjectCommand { Id = id }, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { id }, "Project deleted."));
    }
}
