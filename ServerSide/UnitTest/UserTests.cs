using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ServerSide.Managers.UserManager;
using ServerSide.Core.Authentication.IAuthentication;

namespace UnitTest;

public class UserTests : IClassFixture<TestSetup>, IDisposable
{
    private readonly IUserManager UserManager;
    private readonly IPasswordHashing PasswordHashing;
    private readonly IJWTTokenGenerator JWTTokenGenerator;
    private readonly ApplicationDbContext DbContext;

    /// <summary>
    /// Use the constructor to inject services and setup the test
    /// </summary>
    public UserTests(TestSetup setup)
    {
        PasswordHashing = setup.GetService<IPasswordHashing>();
        JWTTokenGenerator = setup.GetService<IJWTTokenGenerator>();
        DbContext = new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "UserTests")
            .Options
        );

        UserManager = new UserManager(
            new LoggerFactory(),
            new MockEmailService(),
            DbContext,
            JWTTokenGenerator,
            Options.Create(new JWTSettings { Audience = "http://localhost" }),
            PasswordHashing);
    }

    /// <summary>
    /// Get user successful test
    /// </summary>
    [Fact]
    public async Task GetSuccessfulUserTest()
    {
        var password = "Password";
        var hashedPassword = PasswordHashing.HashPassword(password);

        var user = DbContext.Users.Add(new() { FirstName = "FirstName", LastName = "LastName", Password = hashedPassword, Email = "Email" });
        await DbContext.SaveChangesAsync();
        var result = await UserManager.GetUserAsync(new() { Email = "Email", Password = password });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("FirstName", result.Data?.FirstName);

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get user unsuccessful test
    /// </summary>
    [Fact]
    public async Task GetUnsuccessfulUserTest()
    {
        var result = await UserManager.GetUserAsync(new() { Email = "Email1236@gmail.com", Password = "password" });

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    /// <summary>
    /// Invite user successful test
    /// </summary>
    [Fact]
    public async Task InviteUser_ShouldSucceedTest()
    {
        var invite = DbContext.Invitations.Add(new() { Email = "Email", IsAdmin = true, DateCreated = new DateTime().AddDays(10), Token = Guid.NewGuid().ToString() });
        await DbContext.SaveChangesAsync();
        var inviteResult = await UserManager.InviteUser(new() { Email = "example@gamil.com", IsAdmin = true });

        Assert.True(inviteResult.Success);

        DbContext.Invitations.Remove(invite.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Test for inviting a user when no valid invitation exists.
    /// </summary>
    [Fact]
    public async Task InviteUser_ShouldCreateNewInvitation_WhenNoValidInvitationExists()
    {
        var email = "newuser@example.com";

        var result = await UserManager.InviteUser(new InviteUserRequest
        {
            Email = email,
            IsAdmin = false
        });

        Assert.True(result.Success);
        var invite = await DbContext.Invitations.FirstOrDefaultAsync(i => i.Email == email);
        DbContext.Invitations.Remove(invite!);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Test for inviting a user when an invitation already exists.
    /// </summary>
    [Fact]
    public async Task InviteUser_ShouldNotSuccedd_WhenInvitationExists()
    {
        var email = "newuser0183@example.com";

        var invite = DbContext.Invitations.Add(new() { Email = email, IsAdmin = false, DateCreated = DateTime.Now, Token = Guid.NewGuid().ToString() });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.InviteUser(new InviteUserRequest
        {
            Email = email,
            IsAdmin = false
        });

        Assert.False(result.Success);

        DbContext.Invitations.Remove(invite.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Test creating a user from a valid invitation.
    /// </summary>
    [Fact]
    public async Task CreateNewAccount_ShouldReturnSuccess_WhenInvitationIsValid()
    {
        var email = "newuser@example.com";
        var token = Guid.NewGuid().ToString();

        var invite = DbContext.Invitations.Add(new() { Email = email, Token = token, IsAdmin = false, DateCreated = DateTime.UtcNow });
        await DbContext.SaveChangesAsync();

        var request = new CreateNewAccountRequest
        {
            Email = email,
            Password = "pass123",
            InvitationToken = token,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await UserManager.CreateNewAccount(request);

        Assert.True(result.Success);
        Assert.Equal(email, result.Data?.Email);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == result.Data!.Id);
        DbContext.Users.Remove(user!);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Test creating a user from an invalid invitation.
    /// </summary>
    [Fact]
    public async Task CreateNewAccount_ShouldReturnUnsuccess_WhenInvitationInvalid()
    {
        var request = new CreateNewAccountRequest
        {
            Email = "myEmail@email.com",
            Password = "pass123",
            InvitationToken = "tokenRandom22",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await UserManager.CreateNewAccount(request);

        Assert.False(result.Success);
    }

    /// <summary>
    /// Test creating a user from an expired invitation.
    /// </summary>
    [Fact]
    public async Task CreateNewAccount_ShouldReturnUnsuccess_WhenInvitationIsExpired()
    {
        var email = "newuser@example.com";
        var token = Guid.NewGuid().ToString();

        var invite = DbContext.Invitations.Add(new() { Email = email, Token = token, IsAdmin = false, DateCreated = DateTime.Now.AddDays(-33) });
        await DbContext.SaveChangesAsync();

        var request = new CreateNewAccountRequest
        {
            Email = email,
            Password = "pass1237",
            InvitationToken = token,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await UserManager.CreateNewAccount(request);

        Assert.False(result.Success);

        DbContext.Invitations.Remove(invite.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Test creating a user when the email already exists.
    /// </summary>
    [Fact]
    public async Task CreateNewAccount_ShouldReturnUnsuccess_WhenUserExists()
    {
        var email = "newuser01@example.com";
        var token = Guid.NewGuid().ToString();

        DbContext.Users.Add(new User { FirstName = "John", LastName = "Doe", Email = email, Password = "pass1237", IsAdmin = true });

        var invite = DbContext.Invitations.Add(new() { Email = email, Token = token, IsAdmin = true, DateCreated = DateTime.Now });
        await DbContext.SaveChangesAsync();

        var request = new CreateNewAccountRequest
        {
            Email = email,
            Password = "pass1237",
            InvitationToken = token,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await UserManager.CreateNewAccount(request);

        Assert.False(result.Success);

        DbContext.Invitations.Remove(invite.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Login Test with Valid Credentials
    /// </summary>
    [Fact]
    public async Task Login_ShouldSucceed_WithValidCredentials()
    {
        var email = "login@example.com";
        var password = "MyPassword123";
        var hashedPassword = PasswordHashing.HashPassword(password);

        var user = DbContext.Users.Add(new User
        {
            Email = email,
            Password = hashedPassword,
            FirstName = "Log",
            LastName = "In",
            IsAdmin = false
        });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.Login(new()
        {
            Email = email,
            Password = password
        });

        Assert.True(result.Success);
        Assert.Equal(email, result.Data?.Email);
        Assert.False(string.IsNullOrEmpty(result.Data?.Token));

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Login Test with Invalid Credentials
    /// </summary>
    [Fact]
    public async Task Login_ShouldNotSucceed_WithInvalidCredentials()
    {
        var email = "login@example.com";
        var password = "MyPassword123";
        var hashedPassword = PasswordHashing.HashPassword(password);

        var user = DbContext.Users.Add(new User
        {
            Email = email,
            Password = hashedPassword,
            FirstName = "Log",
            LastName = "In",
            IsAdmin = false
        });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.Login(new()
        {
            Email = email,
            Password = "NotSure"
        });

        Assert.False(result.Success);

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// DoesEmailExistAsync should return false when email does not exist
    /// </summary>
    [Fact]
    public async Task DoesEmailExistAsync_ShouldReturnTrue_WhenEmailExists()
    {
        var email = "exist@example.com";
        var user = DbContext.Users.Add(new User
        {
            Email = email,
            FirstName = "Exist",
            LastName = "Check",
            Password = "pass"
        });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.DoesUserExistByEmailAsync(email);

        Assert.True(result.Success);
        Assert.True(result.Data);

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Update User Details should update user details when valid request is made
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateDetails_WhenValidRequest()
    {
        var email = "update@example.com";
        var user = DbContext.Users.Add(new User
        {
            Email = email,
            FirstName = "Old",
            LastName = "Name",
            Password = "pass"
        });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.UpdateUserAsync(new()
        {
            Email = email,
            FirstName = "New",
            LastName = "Name",
            NewPassword = "newpass"
        });

        Assert.True(result.Success);
        Assert.Equal("New", result.Data?.FirstName);

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Update User Details should not update user details when invalid request is made
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_ShouldNotUpdateDetails_WhenInvalidRequest()
    {
        var result = await UserManager.UpdateUserAsync(new()
        {
            Email = "myEmail244@email.com",
            FirstName = "NewName",
            LastName = "NewLastName",
            NewPassword = "newpass04"
        });

        Assert.False(result.Success);
    }

    /// <summary>
    /// Delete User should delete user when valid request is made
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_ShouldSucceed_WhenValidRequest()
    {
        var user = DbContext.Users.Add(new User
        {
            Email = "goodEmail@email.com",
            FirstName = "Jen",
            LastName = "McLard",
            Password = "pass033"
        });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.DeleteUserAsync(new()
        {
            UserId = user.Entity.Id
        });

        Assert.True(result.Success);
    }

    /// <summary>
    /// Delete User should not delete user when invalid request is made
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_ShouldNotSucceed_WhenInvalidRequest()
    {
        var result = await UserManager.DeleteUserAsync(new()
        {
            UserId = Random.Shared.Next(1, 1000),
        });

        Assert.False(result.Success);
    }

    /// <summary>
    /// GetAllEmployees should return all users
    /// </summary>
    [Fact]
    public async Task GetAllEmployees_ShouldReturnAllUsers()
    {
        // Arrange
        DbContext.Users.AddRange(
            new User { Email = "admin@example.com", FirstName = "Admin", LastName = "User", IsAdmin = true, Password = "HashedPassword1" },
            new User { Email = "employee1@example.com", FirstName = "John", LastName = "Doe", IsAdmin = false, Password = "HashedPassword2" },
            new User { Email = "employee2@example.com", FirstName = "Jane", LastName = "Smith", IsAdmin = false, Password = "HashedPassword3" }
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await UserManager.GetAllEmployees();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Data?.Count);

        var users = await DbContext.Users
            .Where(u => u.Email == "admin@example.com" || u.Email == "employee1@example.com" || u.Email == "employee2@example.com")
            .ToListAsync();
        DbContext.Users.RemoveRange(users);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Reset Password Token should send email when user exists
    /// </summary>
    [Fact]
    public async Task ResetPasswordToken_ShouldSendEmail_WhenUserExists()
    {
        var email = "user@example.com";
        var user = DbContext.Users.Add(new User { FirstName = "John", LastName = "Johnson", Email = email, Password = "hashed", IsAdmin = true });
        await DbContext.SaveChangesAsync();

        var result = await UserManager.ResetPasswordToken(email);

        Assert.True(result.Success);

        var resetToken = await DbContext.ResetPasswordRequests.FirstOrDefaultAsync(t => t.Email == email);
        DbContext.ResetPasswordRequests.Remove(resetToken!);
        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Reset Password Token should fail when email is null or empty
    /// </summary>
    [Fact]
    public async Task ResetPasswordToken_ShouldFail_WhenEmailIsNullOrEmpty()
    {
        // Act
        var result = await UserManager.ResetPasswordToken(string.Empty);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email is required!", result.Message);
    }

    /// <summary>
    /// Reset Password Token should fail when email is not found
    /// </summary>
    [Fact]
    public async Task ResetPasswordToken_ShouldFail_WhenEmailNotFound()
    {
        // Act
        var result = await UserManager.ResetPasswordToken("notfound@example.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email not found", result.Message);
    }

    /// <summary>
    /// Reset Password Token should fail when email send fails
    /// </summary>
    [Fact]
    public async Task ResetPasswordToken_ShouldFail_WhenEmailSendFails()
    {
        // Arrange
        var user = DbContext.Users.Add(new User { Email = "fail@example.com", FirstName = "John", LastName = "Doe", Password = "test" });
        await DbContext.SaveChangesAsync();

        // Simulate email failure by overriding SendEmailAsync in a fake implementation
        var fakeEmailService = new FailingEmailService();
        var testUserManager = new UserManager(
            new LoggerFactory(),
            fakeEmailService,
            DbContext,
            JWTTokenGenerator,
            Options.Create(new JWTSettings { Audience = "http://localhost" }),
            PasswordHashing
        );

        // Act
        var result = await testUserManager.ResetPasswordToken("fail@example.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to send email", result.Message);

        DbContext.Users.Remove(user.Entity);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Runs after each test
    /// </summary>
    public void Dispose()
    {
    }
}
