using TaskManagerApi.Communication.Requests;
using TaskManagerApi.Communication.Responses;

namespace TaskManagerApi.Application.Services;

public interface ITaskService
{
    ServiceResult<TaskResponse> Create(TaskRequest request);
    TaskListResponse GetAll();
    ServiceResult<TaskResponse> GetById(Guid id);
    ServiceResult<TaskResponse> Update(Guid id, TaskRequest request);
    ServiceResult Delete(Guid id);
}
