using Microsoft.AspNetCore.Mvc;

namespace TaskManagerApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}
