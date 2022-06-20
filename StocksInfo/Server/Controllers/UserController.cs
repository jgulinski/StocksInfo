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
    private readonly IWatchlistService _watchlistService;

    public UserController(IUserService userService, IWatchlistService watchlistService)
    {
        _userService = userService;
        _watchlistService = watchlistService;
    }

    [Route("register")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserDto user)
    {
        var response = await _userService.RegisterUserAsync(user);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }
        return Ok("User registered");
    }
    
    [Route("signIn")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserDto user)
    {
        var response = await _userService.LoginUserAsync(user);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }
        return Ok(response.Message);
    }

    [HttpGet]
    [Route("{username}/watchList")]
    public async Task<IActionResult> GetWatchlist(string username)
    {
        var response = await _watchlistService.GetWatchlistAsync(username);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok(response.UserWatchlist);
    }
    
    [HttpPost]
    [Route("{username}/watchlist/{ticker}")]
    public async Task<IActionResult> AddToWatchlist(string username, string ticker)
    {
        var response = await _watchlistService.AddToWatchlistAsync(username, ticker);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok();
    }

    [HttpDelete]
    [Route("{username}/watchlist/{ticker}")]
    public async Task<IActionResult> DeleteFromWatchlist(string username, string ticker)
    {
        var response = await _watchlistService.DeleteFromWatchlistAsync(username, ticker);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok();
    }
}