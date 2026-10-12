namespace TaskManagerApi.Application.Services;

public enum ServiceResultStatus
{
    Success,
    ValidationError,
    NotFound
}

public sealed record ServiceResult<T>(
    ServiceResultStatus Status,
    T? Data,
    IReadOnlyList<string> Errors)
    where T : class;

public sealed record ServiceResult(
    ServiceResultStatus Status,
    IReadOnlyList<string> Errors);
