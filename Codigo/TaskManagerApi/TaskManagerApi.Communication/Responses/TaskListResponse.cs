namespace TaskManagerApi.Communication.Responses;

public sealed class TaskListResponse
{
    public List<TaskResponse> Tasks { get; set; } = [];
}
