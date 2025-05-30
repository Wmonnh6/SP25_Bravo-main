using ServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("get")]
    public ManagerResult<string> GetUserAsync()
    {
        return ManagerResult<string>.Successful("The API is working");
    }

    [HttpGet("getSecured")]
    [Authorize]
    public ManagerResult<string> GetUserSecuredAsync()
    {
        return ManagerResult<string>.Successful("The API is working");
    }
}
