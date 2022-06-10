using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.Models;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Route("register")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserDto user)
    {
        StatusResponse response = await _userService.RegisterUserAsync(user);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Content);
        }
        return Ok("User registered");
    }
    
    [Route("login")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserDto user)
    {
        StatusResponse response = await _userService.LoginUserAsync(user);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Content);
        }
        return Ok(response.Content);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        StatusResponse response = await _userService.RefreshAccessTokenAsync(refreshToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Content);

        }
        return Ok(response.Content);
    }
}