using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Perfect.Application.Common;

namespace Perfect.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _clock;

    public TokenService(IOptions<JwtOptions> options, IDateTimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public string GenerateAccessToken(TokenDescriptor descriptor)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, descriptor.UserId.ToString()),
            new("tenantId", descriptor.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Email, descriptor.Email)
        };

        claims.AddRange(descriptor.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(descriptor.Permissions.Select(permission => new Claim("perm", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ResolveSigningKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _clock.UtcNow,
            expires: _clock.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);
        return new RefreshTokenResult(token, _clock.UtcNow.AddDays(_options.RefreshTokenDays));
    }
}
