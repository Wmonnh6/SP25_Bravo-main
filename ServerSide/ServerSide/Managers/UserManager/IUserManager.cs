using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;

namespace ServerSide.Managers.UserManager;

public interface IUserManager
{
    Task<ManagerResult<List<UserSelectionDTO>>> GetAllEmployees();
    Task<ManagerResult<UserDTO>> AddUserAsync(CreateNewAccountRequest request);
    Task<ManagerResult<int>> DeleteUserAsync(DeleteUserRequest request);
    Task<ManagerResult<UserDTO>> GetUserAsync(LoginRequest request);
    Task<ManagerResult<UserDTO>> UpdateUserAsync(UpdateUserRequest request);
    Task<ManagerResult<UserDTO>> Login(LoginRequest request);
    Task<ManagerResult<UserDTO>> CreateNewAccount(CreateNewAccountRequest request);
    Task<ManagerResult<bool>> InviteUser(InviteUserRequest request);
    
    // Password Reset
    Task<ManagerResult<bool>> DoesUserExistByEmailAsync(string email);
    Task<ManagerResult<bool>> ResetPasswordToken(string email);
    Task<ManagerResult<bool>> ResetPassword(Requests.ResetPasswordRequest request);
}
