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

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
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
}
