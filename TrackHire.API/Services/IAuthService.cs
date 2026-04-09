using TrackHire.API.DTOs;
namespace TrackHire.API.Services;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto);
}
