using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Application.Services;
using TaskManagerApi.Communication.Requests;
using TaskManagerApi.Communication.Responses;

namespace TaskManagerApi.Api.Controllers;

public sealed class TasksController : ApiControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TaskRequest request)
    {
        var result = _taskService.Create(request);

        if (result.Status == ServiceResultStatus.ValidationError)
        {
            return BadRequest(ToErrorResponse(result.Errors));
        }

        var response = result.Data!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var response = _taskService.GetAll();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var result = _taskService.GetById(id);

        if (result.Status == ServiceResultStatus.NotFound)
        {
            return NotFound(ToErrorResponse(result.Errors));
        }

        return Ok(result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(
        [FromRoute] Guid id,
        [FromBody] TaskRequest request)
    {
        var result = _taskService.Update(id, request);

        if (result.Status == ServiceResultStatus.NotFound)
        {
            return NotFound(ToErrorResponse(result.Errors));
        }

        if (result.Status == ServiceResultStatus.ValidationError)
        {
            return BadRequest(ToErrorResponse(result.Errors));
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var result = _taskService.Delete(id);

        if (result.Status == ServiceResultStatus.NotFound)
        {
            return NotFound(ToErrorResponse(result.Errors));
        }

        return NoContent();
    }

    private static ErrorResponse ToErrorResponse(IReadOnlyList<string> errors) => new()
    {
        Errors = errors.ToList()
    };
}
