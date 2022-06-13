using Shared.Models;

namespace Server.Services;

public interface IUserService
{
    Task<StatusResponse> RegisterUserAsync(UserDto user);
    Task<StatusResponse> LoginUserAsync(UserDto user);
}