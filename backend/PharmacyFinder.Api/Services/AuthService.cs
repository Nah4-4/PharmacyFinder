using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyFinder.Api.Data;
using PharmacyFinder.Api.DTOs;
using PharmacyFinder.Api.Enums;
using PharmacyFinder.Api.Models;

namespace PharmacyFinder.Api.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly PFContext _context;
    private readonly IJwtTokenService _tokenService;

    public AuthService(
        UserManager<User> userManager,
        PFContext context,
        IJwtTokenService tokenService
    )
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
            return ServiceResult<AuthResponseDto>.Conflict(
                "An account with this email already exists."
            );

        var user = new User
        {
            UserName = email,
            Email = email,
            FullName = dto.FullName.Trim(),
            Role = dto.Role == UserRole.Admin ? UserRole.Customer : dto.Role,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ServiceResult<AuthResponseDto>.Failure(
                string.Join(" ", result.Errors.Select(error => error.Description))
            );

        _context.AuditLogs.Add(
            new AuditLog
            {
                UserId = user.Id,
                Action = "USER_REGISTERED",
                EntityName = "User",
            }
        );
        await _context.SaveChangesAsync();

        return ServiceResult<AuthResponseDto>.Success(
            _tokenService.GenerateToken(user),
            "Registration successful."
        );
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return ServiceResult<AuthResponseDto>.Unauthorized("Invalid email or password.");

        return ServiceResult<AuthResponseDto>.Success(
            _tokenService.GenerateToken(user),
            "Login successful."
        );
    }

    public Task<CurrentUserDto?> GetCurrentUserAsync(int userId) =>
        _context
            .Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new CurrentUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
            })
            .FirstOrDefaultAsync();

    public Task<List<User>> GetAllAsync() => _userManager.Users.AsNoTracking().ToListAsync();
}

public sealed record ServiceResult<T>(T? Data, ServiceResultStatus Status, string Message)
{
    public static ServiceResult<T> Success(T data, string message) =>
        new(data, ServiceResultStatus.Success, message);

    public static ServiceResult<T> Conflict(string message) =>
        new(default, ServiceResultStatus.Conflict, message);

    public static ServiceResult<T> Unauthorized(string message) =>
        new(default, ServiceResultStatus.Unauthorized, message);

    public static ServiceResult<T> Failure(string message) =>
        new(default, ServiceResultStatus.Failure, message);
}

public enum ServiceResultStatus
{
    Success,
    Conflict,
    Unauthorized,
    Failure,
}
