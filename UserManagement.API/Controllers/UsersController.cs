using Microsoft.AspNetCore.Mvc;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found." });
        return Ok(user);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var users = await _userService.GetActiveUsersAsync();
        return Ok(users);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var users = await _userService.GetInactiveUsersAsync();
        return Ok(users);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.RegisterUserAsync(createUserDto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found.")
                return NotFound(new { message = ex.Message });
            
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found.")
                return NotFound(new { message = ex.Message });
            
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        try
        {
            await _userService.ActivateUserAsync(id);
            return Ok(new { message = "User activated successfully." });
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found.")
                return NotFound(new { message = ex.Message });
            
            return BadRequest(new { message = ex.Message });
        }
    }
}
