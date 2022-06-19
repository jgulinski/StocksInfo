using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Server.Helpers;
using Shared.Models;

namespace Server.Services;

public class UserService : IUserService
{
    private readonly ApiContext _context;
    private readonly IConfiguration _configuration;

    public UserService(ApiContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
  public async Task<StatusResponse> RegisterUserAsync(UserDto user)
    {
        User newUser = new User();

        var userWithGivenEmail =
            await _context.Users.FirstOrDefaultAsync((e => e.EmailAddress == user.EmailAddress));

        if (userWithGivenEmail != null)
        {
            return new StatusResponse()
                {StatusCode = HttpStatusCode.BadRequest, Message = "User with given email already exists"};
        }

        string hashedPassword = new PasswordHasher<User>()
            .HashPassword(newUser, user.Password);

        await _context.Users.AddAsync(new User()
        {
            EmailAddress = user.EmailAddress,
            Password = hashedPassword,
        });

        await _context.SaveChangesAsync();

        return new StatusResponse() {StatusCode = HttpStatusCode.OK};
    }


  public async Task<StatusResponse> LoginUserAsync(UserDto user)
    {
        var userFromDb = await _context.Users.Where(e => e.EmailAddress == user.EmailAddress).FirstOrDefaultAsync();

        if (userFromDb == null)
        {
            return new StatusResponse()
                {StatusCode = HttpStatusCode.BadRequest, Message = "User with given email not found"};
        }

        PasswordVerificationResult verifiedPassword = new PasswordHasher<User>()
            .VerifyHashedPassword(userFromDb, userFromDb.Password, user.Password);

        if (verifiedPassword == PasswordVerificationResult.Failed)
        {
            return new StatusResponse() {StatusCode = HttpStatusCode.BadRequest, Message = "Wrong password"};
        }
        var token = GenerateToken(userFromDb);
        
        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK, Message = token
        };
    }
  private string GenerateToken(User user)
  {
      var token = AuthHelper.GenerateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Key"]);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}