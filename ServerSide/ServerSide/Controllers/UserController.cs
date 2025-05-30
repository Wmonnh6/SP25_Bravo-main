using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Managers.UserManager;
using Microsoft.AspNetCore.Authorization;

namespace ServerSide.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    [HttpGet("get-all-employees")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<List<UserSelectionDTO>>> GetAllEmployees([FromServices] IUserManager userManager)
    {
        return await userManager.GetAllEmployees();
    }

    [HttpPut("addUser")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<UserDTO>> AddUserAsync([FromServices] IUserManager userManager, [FromBody] CreateNewAccountRequest request)
    {
        return await userManager.AddUserAsync(request);
    }

    [HttpPut("getUser")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<UserDTO>> GetUserAsync([FromServices] IUserManager userManager, [FromBody] LoginRequest request)
    {
        return await userManager.GetUserAsync(request);
    }

    [HttpPut("update-user")]
    [Authorize]
    public async Task<ManagerResult<UserDTO>> UpdateUserAsync([FromServices] IUserManager userManager, [FromBody] UpdateUserRequest request)
    {
        return await userManager.UpdateUserAsync(request);
    }

    [HttpDelete("deleteUser")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<int>> DeleteUserAsync([FromServices] IUserManager userManager, [FromBody] DeleteUserRequest request)
    {
        return await userManager.DeleteUserAsync(request);
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<ManagerResult<UserDTO>> Register([FromServices] IUserManager userManager, [FromBody] CreateNewAccountRequest request)
    {
        return await userManager.CreateNewAccount(request);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ManagerResult<UserDTO>> Login([FromServices] IUserManager userManager, [FromBody] LoginRequest request)
    {
        return await userManager.Login(request);
    }

    [HttpPost("invite-user")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<bool>> InviteUser([FromServices] IUserManager userManager, [FromBody] InviteUserRequest request)
    {
        return await userManager.InviteUser(request);
    }

    [HttpGet("checkEmailExists")]
    [AllowAnonymous]
    public async Task<ManagerResult<bool>> CheckEmailExists([FromServices] IUserManager userManager, [FromQuery] string email)
    {
        return await userManager.ResetPasswordToken(email);
    }

    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<ManagerResult<bool>> ResetPassword([FromServices] IUserManager userManager, [FromBody] Requests.ResetPasswordRequest request)
    {
        return await userManager.ResetPassword(request);
    }
}
