using System.Security.Claims;
using System.Text;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Gera tokens JWT assinados com HMAC-SHA256.
/// </summary>
public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    /// <summary>Nomes das claims gravadas no token (padrão JWT, RFC 7519).</summary>
    public static class Claims
    {
        public const string Id = JwtRegisteredClaimNames.Sub;
        public const string Nome = JwtRegisteredClaimNames.Name;
        public const string Email = JwtRegisteredClaimNames.Email;
        public const string Perfil = "role";
    }

    private readonly JsonWebTokenHandler _handler = new();

    public TokenGerado GerarToken(Usuario usuario)
    {
        var config = options.Value;
        var agora = timeProvider.GetUtcNow();
        var expiraEm = agora.AddMinutes(config.ExpiracaoMinutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = config.Emissor,
            Audience = config.Audiencia,
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(Claims.Id, usuario.Id.ToString()),
                new Claim(Claims.Nome, usuario.Nome),
                new Claim(Claims.Email, usuario.Email),
                new Claim(Claims.Perfil, usuario.Perfil.ToApiString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            SigningCredentials = new SigningCredentials(CriarChave(config.ChaveSecreta), SecurityAlgorithms.HmacSha256)
        };

        return new TokenGerado(_handler.CreateToken(descriptor), expiraEm);
    }

    public static SymmetricSecurityKey CriarChave(string chaveSecreta) =>
        new(Encoding.UTF8.GetBytes(chaveSecreta));
}
