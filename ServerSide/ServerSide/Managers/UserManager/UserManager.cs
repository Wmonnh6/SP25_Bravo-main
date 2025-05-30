using ServerSide.DTOs;
using System.Net.Mail;
using ServerSide.Models;
using ServerSide.Mapper;
using ServerSide.Requests;
using ServerSide.Models.Entities;
using ServerSide.DatabaseContext;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerSide.Core.Services.IServices;
using ServerSide.Core.Authentication.IAuthentication;

namespace ServerSide.Managers.UserManager;

public class UserManager : BaseManager, IUserManager
{
    private readonly IPasswordHashing PasswordHashing;
    private readonly IEmailService EmailService;
    private readonly IJWTTokenGenerator JWTTokenGenerator;
    private readonly JWTSettings JwtSettings;

    public UserManager(ILoggerFactory logger,
        IEmailService emailService,
        ApplicationDbContext dbContext,
        IJWTTokenGenerator jWTTokenGenerator,
        IOptions<JWTSettings> jwtSettings,
        IPasswordHashing passwordHashing) : base(logger, dbContext)
    {
        EmailService = emailService;
        PasswordHashing = passwordHashing;
        JWTTokenGenerator = jWTTokenGenerator;
        JwtSettings = jwtSettings.Value;
    }

    public async Task<ManagerResult<List<UserSelectionDTO>>> GetAllEmployees()
    {
        var employees = await DbContext.Users.ToListAsync();

        return ManagerResult<List<UserSelectionDTO>>.Successful("See results!", employees.Select(x => x.UserSelectionDTO()).ToList());
    }

    public async Task<ManagerResult<UserDTO>> CreateNewAccount(CreateNewAccountRequest userDTO)
    {
        var invitation = await DbContext.Invitations.FirstOrDefaultAsync(x => x.Email.ToLower() == userDTO.Email.ToLower() && x.Token == userDTO.InvitationToken);
        if (invitation is null)
        {
            return ManagerResult<UserDTO>.Unsuccessful("Invalid token or email!");
        }

        // Check if the invitation has expired (more than 24 hours)
        if (invitation.DateCreated.AddHours(24) < DateTime.Now)
        {
            return ManagerResult<UserDTO>.Unsuccessful("Expired Invitation");
        }

        // Check if a user with the same email already exists in the Users table
        var existingUser = await DbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == userDTO.Email.ToLower());
        if (existingUser != null)
        {
            return ManagerResult<UserDTO>.Unsuccessful("A user with this email already exists.");
        }

        userDTO.InvitationToken = invitation.Token;
        userDTO.IsAdmin = invitation.IsAdmin;

        var dbUser = await AddUserAsync(userDTO);
        if (!dbUser.Success)
            return ManagerResult<UserDTO>.Unsuccessful(dbUser.Message);

        DbContext.Invitations.Remove(invitation);
        await DbContext.SaveChangesAsync();

        string token = JWTTokenGenerator.GenerateJwtToken(dbUser.Data.Id, dbUser.Data.FirstName, dbUser.Data.LastName, userDTO.Email, invitation.IsAdmin);

        var result = new UserDTO()
        {
            Id = dbUser.Data.Id,
            FirstName = dbUser.Data.FirstName,
            LastName = dbUser.Data.LastName,
            Email = dbUser.Data.Email,
            Token = token
        };

        return ManagerResult<UserDTO>.Successful("User Created Successfully!", result);
    }

    public async Task<ManagerResult<UserDTO>> Login(LoginRequest userDTO)
    {
        var user = await GetUserAsync(userDTO);
        if (!user.Success)
            return ManagerResult<UserDTO>.Unsuccessful(user.Message);

        string token = JWTTokenGenerator.GenerateJwtToken(user.Data.Id, user.Data.FirstName, user.Data.LastName, user.Data.Email, user.Data.IsAdmin);

        var result = new UserDTO()
        {
            Id = user.Data.Id,
            FirstName = user.Data.FirstName,
            LastName = user.Data.LastName,
            Email = user.Data.Email,
            Token = token
        };

        return ManagerResult<UserDTO>.Successful("Successfully generated the JWT token!", result);
    }

    public async Task<ManagerResult<UserDTO>> GetUserAsync(LoginRequest userDTO)
    {
        var hashiedPassword = PasswordHashing.HashPassword(userDTO.Password);
        User? user = await DbContext.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email && x.Password == hashiedPassword);
        if (user == null)
        {
            _logger.LogError("Couldn't find the user");
            return ManagerResult<UserDTO>.Unsuccessful("Invalid email or password.");
        }

        return ManagerResult<UserDTO>.Successful("User found!", user.MapUserToUserDTO());
    }

    public async Task<ManagerResult<UserDTO>> AddUserAsync(CreateNewAccountRequest request)
    {
        var hashiedPassword = PasswordHashing.HashPassword(request.Password);
        User userDto = new() { Password = hashiedPassword, FirstName = request.FirstName, LastName = request.LastName, Email = request.Email, IsAdmin = request.IsAdmin};
        DbContext.Users.Add(userDto);
        await DbContext.SaveChangesAsync();

        return ManagerResult<UserDTO>.Successful("Added!", userDto.MapUserToUserDTO());
    }

    public async Task<ManagerResult<UserDTO>> UpdateUserAsync(UpdateUserRequest request)
    {
        User? user = await DbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            _logger.LogError("Couldn't find the user");
            return ManagerResult<UserDTO>.Unsuccessful("Couldn't find the user");
        }

        // only hash and store a password change if a new (validated) password was included
        if(!request.NewPassword.IsNullOrEmpty())
        {
			var hashedPassword = PasswordHashing.HashPassword(request.NewPassword);
			user.Password = hashedPassword;
		}
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await DbContext.SaveChangesAsync();

        string token = JWTTokenGenerator.GenerateJwtToken(user.Id, user.FirstName, user.LastName, user.Email, user.IsAdmin);
        var userDto = user.MapUserToUserWithTokenDTO(token);

        return ManagerResult<UserDTO>.Successful("User updated!", userDto);
    }

    public async Task<ManagerResult<int>> DeleteUserAsync(DeleteUserRequest request)
    {
        User? user = await DbContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
        if (user == null)
        {
            _logger.LogError("Couldn't find the user");
            return ManagerResult<int>.Unsuccessful("Couldn't find the user");
        }

        DbContext.Users.Remove(user);
        await DbContext.SaveChangesAsync();

        return ManagerResult<int>.Successful("User deleted!", user.Id);
    }

    /// <summary>
    /// Make sure the invitation exists and not expired.
    /// If expired remove the old one and create a new one.
    /// </summary>
    public async Task<ManagerResult<bool>> InviteUser(InviteUserRequest request)
    {
        var currentUser = await DbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower());
        if (currentUser != null)
        {
            return ManagerResult<bool>.Unsuccessful("User already exists!", false);
        }

        var invitation = await DbContext.Invitations.FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower());

        // if the invitation already exists
        //    if the invitation has not expired
        //       return that an invitation already exists for that email
        //    else
        //       delete the invitation
        // generate invitation
        // email invitation
        // if email unsuccessful
        //    delete the invitation
        //    return that the was an issue sending the email, try again
        // else
        //    return that invitation was successfully created
        if (invitation is not null)
        {
            // subtracting the current DateTime from the DateCreated will always be a negative number; inverted the equation and it works now
            if(DateTime.Now.Subtract(invitation.DateCreated).Days <= 1)
            {
                return ManagerResult<bool>.Unsuccessful("Invitation already exists!", false);
            } else
            {
                DbContext.Invitations.Remove(invitation); // email is expired so a new one will need to be created
            }
        }

        // creating the new invite object so that we can reference its information for the email message
        var newInvitation = new Invitation {
			DateCreated = DateTime.Now,
			Email = request.Email,
			IsAdmin = request.IsAdmin,
			Token = Guid.NewGuid().ToString()
		};
        //DbContext.Invitations.Add(new() { DateCreated = DateTime.Now, Email = request.Email, IsAdmin = request.IsAdmin, Token = Guid.NewGuid().ToString() });
        DbContext.Invitations.Add(newInvitation);
        await DbContext.SaveChangesAsync();

		// add email service
		// create the Message with recipient
		// add error checking in case the email fails
        string msgString = $"You have been invited to register for the My Time Entry system.\nYou may access the registration page through the <a href='{JwtSettings.Audience}/register'>link</a>.<br/><br/>Your invitation code is: {newInvitation.Token}";
		MailMessage message = new MailMessage
        {
            Subject = "Invitation to My Time Entry",
            Body = msgString,
            IsBodyHtml = true
        };
        message.To.Add(newInvitation.Email);
        var succeeded = await EmailService.SendEmailAsync(message);

        if (!succeeded) {
            DbContext.Remove(newInvitation); // to clean up so that the unsent "valid" invitation won't stop the user from trying again
            return ManagerResult<bool>.Unsuccessful("There was an issue sending the invitation. Please try again.", false);
        }

        return ManagerResult<bool>.Successful("Invitation created!", true);
    }

    public async Task<ManagerResult<bool>> ResetPasswordToken(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return ManagerResult<bool>.Unsuccessful("Email is required!", false);
        }

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return ManagerResult<bool>.Unsuccessful("Email not found", false);
        }

        string resetToken = Guid.NewGuid().ToString();

        var resetRequest = new Models.Entities.ResetPasswordRequest
        {
            Token = resetToken,
            DateCreated = DateTime.UtcNow,
            Email = email
        };

        DbContext.ResetPasswordRequests.Add(resetRequest);
        await DbContext.SaveChangesAsync();

        var mailMessage = new MailMessage
        {
            Subject = "Password Reset Request",
            Body = $"Click the link to reset your password: <a href='{JwtSettings.Audience}/set-new-password?token={resetToken}'>Reset Password</a>",
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        bool emailSent = await EmailService.SendEmailAsync(mailMessage);
        if (!emailSent)
        {
            return ManagerResult<bool>.Unsuccessful("Failed to send email", false);
        }

        return ManagerResult<bool>.Successful("Password reset email sent successfully!", false);
    }

    public async Task<ManagerResult<bool>> DoesEmailExistAsync(string email)
    {
        bool exists = await DbContext.Users.AnyAsync(u => u.Email == email);
        return exists
            ? ManagerResult<bool>.Successful("Email exists.", true)
            : ManagerResult<bool>.Unsuccessful("Email not found.");
    }

    public async Task<ManagerResult<bool>> DoesUserExistByEmailAsync(string email)
    {
        // Check if that email is assigned to any users in the datbase
        bool exists = await DbContext.Users.AnyAsync(u => u.Email == email);

        // Depending on existence of email return methods found in ManagerResult
        return exists ? ManagerResult<bool>.Successful("Email exists", true)
                      : ManagerResult<bool>.Unsuccessful("Email not found");
    }

    public async Task<ManagerResult<bool>> ResetPassword(Requests.ResetPasswordRequest request)
    {
        //Verify the Reset Token
        var resetPWRequest = await DbContext.ResetPasswordRequests.FirstOrDefaultAsync(x => x.Token == request.ResetToken);

        if (resetPWRequest is null)
        {
            return ManagerResult<bool>.Unsuccessful("Reset Token is invalid.", false);
        }

        if (DateTime.Now.Subtract(resetPWRequest.DateCreated).Minutes > 30)
        {
            return ManagerResult<bool>.Unsuccessful("Reset Token is expired.", false);
        }

        //To find the user, get their email from the ResetPasswordRequests table
        User? user = await DbContext.Users.FirstOrDefaultAsync(x => x.Email == resetPWRequest.Email);

        if (user is null)
        {
            return ManagerResult<bool>.Unsuccessful("Could not find your user account.", false);
        }

        //Change the User's password to the new one
        user.Password = PasswordHashing.HashPassword(request.newPassword);
        DbContext.Remove(resetPWRequest);
        await DbContext.SaveChangesAsync();

        return ManagerResult<bool>.Successful("Password reset successfully.", true);
    }
}
