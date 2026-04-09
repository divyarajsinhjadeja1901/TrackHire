using TrackHire.API.Data;
using TrackHire.API.DTOs;
using TrackHire.API.Helpers;
using TrackHire.API.Models;
using Microsoft.EntityFrameworkCore;

namespace TrackHire.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthService(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<string?> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return null; 
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) 
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return _jwtHelper.GenerateToken(user); 
    }
    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;
        return _jwtHelper.GenerateToken(user);
    }
}