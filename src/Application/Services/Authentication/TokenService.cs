using Application.Interfaces.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Authentication;

public class TokenService : ITokenService
{
	private readonly IConfiguration _config;
	private readonly string _issuer;
	private readonly string _audience;
	private readonly string _key;

	public TokenService(IConfiguration config)
	{
		_config = config;
		_issuer = _config["Jwt:Issuer"] ??
			throw new ArgumentNullException("Jwt:Issuer");
		_audience = _config["Jwt:Audience"] ??
			throw new ArgumentNullException("Jwt:Audience");
		_key = _config["Jwt:Key"] ??
			throw new ArgumentNullException("Jwt:Key");
	}

	public string GenerateToken(string userId, string userName, string role, int expiresMinutes = 60)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId),
			new Claim(JwtRegisteredClaimNames.UniqueName, userName),
			new Claim(ClaimTypes.Role, role)
		};

		var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
		var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _issuer,
			audience: _audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}