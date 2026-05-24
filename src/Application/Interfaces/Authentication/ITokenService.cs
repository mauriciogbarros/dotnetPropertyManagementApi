namespace Application.Interfaces.Authentication;

public interface ITokenService
{
	string GenerateToken(string userId, string userName, string role, int expiresMinutes = 60);
}