namespace TaskManagerApi.Communication.Responses;

public sealed class ErrorResponse
{
    public List<string> Errors { get; set; } = [];
}
