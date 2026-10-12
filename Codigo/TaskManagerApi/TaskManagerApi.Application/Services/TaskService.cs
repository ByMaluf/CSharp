using TaskManagerApi.Application.Entities;
using TaskManagerApi.Communication.Requests;
using TaskManagerApi.Communication.Responses;

namespace TaskManagerApi.Application.Services;

public sealed class TaskService : ITaskService
{
    private static readonly Dictionary<string, string> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["high"] = "high",
        ["medium"] = "medium",
        ["low"] = "low"
    };

    private static readonly Dictionary<string, string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pending"] = "pending",
        ["inProgress"] = "inProgress",
        ["completed"] = "completed"
    };

    private readonly List<TaskEntity> _tasks = [];
    private readonly object _sync = new();

    public ServiceResult<TaskResponse> Create(TaskRequest request)
    {
        var errors = ValidateRequest(request);

        if (errors.Count > 0)
        {
            return new ServiceResult<TaskResponse>(
                ServiceResultStatus.ValidationError,
                null,
                errors);
        }

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = NormalizeDescription(request.Description),
            Priority = NormalizePriority(request.Priority),
            DueDate = request.DueDate,
            Status = NormalizeStatus(request.Status)
        };

        lock (_sync)
        {
            _tasks.Add(task);
        }

        return new ServiceResult<TaskResponse>(
            ServiceResultStatus.Success,
            ToResponse(task),
            []);
    }

    public TaskListResponse GetAll()
    {
        lock (_sync)
        {
            return new TaskListResponse
            {
                Tasks = _tasks
                    .OrderBy(task => task.DueDate)
                    .ThenBy(task => task.Name)
                    .Select(ToResponse)
                    .ToList()
            };
        }
    }

    public ServiceResult<TaskResponse> GetById(Guid id)
    {
        lock (_sync)
        {
            var task = _tasks.FirstOrDefault(task => task.Id == id);

            if (task is null)
            {
                return new ServiceResult<TaskResponse>(
                    ServiceResultStatus.NotFound,
                    null,
                    [$"Tarefa com ID '{id}' não encontrada."]);
            }

            return new ServiceResult<TaskResponse>(
                ServiceResultStatus.Success,
                ToResponse(task),
                []);
        }
    }

    public ServiceResult<TaskResponse> Update(Guid id, TaskRequest request)
    {
        lock (_sync)
        {
            var task = _tasks.FirstOrDefault(task => task.Id == id);

            if (task is null)
            {
                return new ServiceResult<TaskResponse>(
                    ServiceResultStatus.NotFound,
                    null,
                    [$"Tarefa com ID '{id}' não encontrada."]);
            }

            var errors = ValidateRequest(request);

            if (errors.Count > 0)
            {
                return new ServiceResult<TaskResponse>(
                    ServiceResultStatus.ValidationError,
                    null,
                    errors);
            }

            task.Name = request.Name.Trim();
            task.Description = NormalizeDescription(request.Description);
            task.Priority = NormalizePriority(request.Priority);
            task.DueDate = request.DueDate;
            task.Status = NormalizeStatus(request.Status);

            return new ServiceResult<TaskResponse>(
                ServiceResultStatus.Success,
                ToResponse(task),
                []);
        }
    }

    public ServiceResult Delete(Guid id)
    {
        lock (_sync)
        {
            var task = _tasks.FirstOrDefault(task => task.Id == id);

            if (task is null)
            {
                return new ServiceResult(
                    ServiceResultStatus.NotFound,
                    [$"Tarefa com ID '{id}' não encontrada."]);
            }

            _tasks.Remove(task);

            return new ServiceResult(
                ServiceResultStatus.Success,
                []);
        }
    }

    private static List<string> ValidateRequest(TaskRequest request)
    {
        var errors = new List<string>();
        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("O nome da tarefa é obrigatório.");
        }
        else if (name.Length > 100)
        {
            errors.Add("O nome da tarefa deve ter no máximo 100 caracteres.");
        }

        if (request.Description?.Trim().Length > 500)
        {
            errors.Add("A descrição da tarefa deve ter no máximo 500 caracteres.");
        }

        if (request.DueDate < DateTime.Now)
        {
            errors.Add("A data limite da tarefa não pode estar no passado.");
        }

        if (string.IsNullOrWhiteSpace(request.Priority) ||
            !AllowedPriorities.ContainsKey(request.Priority.Trim()))
        {
            errors.Add("A prioridade deve ser high, medium ou low.");
        }

        if (string.IsNullOrWhiteSpace(request.Status) ||
            !AllowedStatuses.ContainsKey(request.Status.Trim()))
        {
            errors.Add("O status deve ser pending, inProgress ou completed.");
        }

        return errors;
    }

    private static string NormalizePriority(string priority) =>
        AllowedPriorities[priority.Trim()];

    private static string NormalizeStatus(string status) =>
        AllowedStatuses[status.Trim()];

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return description.Trim();
    }

    private static TaskResponse ToResponse(TaskEntity task) => new()
    {
        Id = task.Id,
        Name = task.Name,
        Description = task.Description,
        Priority = task.Priority,
        DueDate = task.DueDate,
        Status = task.Status
    };
}
