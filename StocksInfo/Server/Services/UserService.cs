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
                {StatusCode = HttpStatusCode.BadRequest, Content = "User with given email already exists"};
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
                {StatusCode = HttpStatusCode.BadRequest, Content = "User with given email not found"};
        }

        PasswordVerificationResult verifiedPassword = new PasswordHasher<User>()
            .VerifyHashedPassword(userFromDb, userFromDb.Password, user.Password);

        if (verifiedPassword == PasswordVerificationResult.Failed)
        {
            return new StatusResponse() {StatusCode = HttpStatusCode.BadRequest, Content = "Wrong password"};
        }

        if (verifiedPassword == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // string hashedPassword = new PasswordHasher<User>()
            // .HashPassword(userFromDb, );
        }

        var tokens = await GenerateTokensAndUpdateUserTokens(userFromDb);


        return new StatusResponse()
        {
            // StatusCode = HttpStatusCode.OK, Content = new {accessToken = tokens[0], refreshToken = tokens[1]}
            StatusCode = HttpStatusCode.OK, Content = tokens[0]
        };
    }

    public async Task<StatusResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        var user = await _context.Users.Where(e => e.RefreshToken == refreshToken).FirstOrDefaultAsync();

        if (user == null)
        {
            return new StatusResponse()
                {StatusCode = HttpStatusCode.BadRequest, Content = "refresh token doesn't exist"};
        }

        if (user.RefreshTokenExpirationDateTime < DateTime.Now)
        {
            return new StatusResponse() {StatusCode = HttpStatusCode.BadRequest, Content = "refresh token expired"};
        }
        
        
        var token = AuthHelper.GenerateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Key"]);


        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK, Content = token
        };
    }

    private async Task<string[]> GenerateTokensAndUpdateUserTokens(User user)
    {
        var token = AuthHelper.GenerateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Key"]);
        var refreshToken = Guid.NewGuid();

        user.RefreshToken = refreshToken.ToString();
        user.RefreshTokenExpirationDateTime = DateTime.Now.AddMinutes(20);

        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return new[] {new JwtSecurityTokenHandler().WriteToken(token), refreshToken.ToString()};
    }
}