using KzBarry.Models.DTOs.Auth;

namespace KzBarry.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterUser(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> Refresh(RefreshRequest request, string userIdClaim);
        Task Logout(RefreshRequest request);
    }
}
