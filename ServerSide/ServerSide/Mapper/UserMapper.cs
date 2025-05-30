using ServerSide.DTOs;
using ServerSide.Models.Entities;

namespace ServerSide.Mapper;

public static class UserMapper
{
    public static UserDTO MapUserToUserDTO(this User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsAdmin = user.IsAdmin
        };
    }

    public static UserDTO MapUserToUserWithTokenDTO(this User user, string token)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            Token = token
        };
    }

    public static UserSelectionDTO UserSelectionDTO(this User user)
    {
        return new UserSelectionDTO
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}"
        };
    }
}
