using JpHelpDesk.Api.Models;

namespace JpHelpDesk.Api.Services.Security;

public record TokenGerado(string Token, DateTimeOffset ExpiraEm);

public interface ITokenService
{
    TokenGerado GerarToken(Usuario usuario);
}
