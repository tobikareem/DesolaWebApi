using DesolaDomain.Model;

namespace DesolaServices.Interfaces;

public interface IAuthService
{
    Task<AuthenticationToken> ExchangeCodeForTokenAsync(string code);
    Task<AuthenticationToken> RefreshTokensAsync(string refreshToken);
}