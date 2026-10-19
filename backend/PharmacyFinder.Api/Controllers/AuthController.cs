using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyFinder.Api.DTOs;
using PharmacyFinder.Api.Services;

namespace PharmacyFinder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.Status == ServiceResultStatus.Conflict)
            return Conflict(ApiResponse<AuthResponseDto>.Fail(result.Message));
        if (result.Status == ServiceResultStatus.Failure)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(result.Message));

        return CreatedAtAction(
            nameof(GetCurrentUser),
            null,
            ApiResponse<AuthResponseDto>.Ok(result.Data!, result.Message));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result.Status == ServiceResultStatus.Unauthorized)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Message));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!, result.Message));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse<CurrentUserDto>.Fail("Invalid token."));

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user is null)
            return NotFound(ApiResponse<CurrentUserDto>.Fail("User not found."));

        return Ok(ApiResponse<CurrentUserDto>.Ok(user));
    }

    [HttpGet]
    public async Task<ActionResult> GetAll() =>
        Ok(await _authService.GetAllAsync());
}
