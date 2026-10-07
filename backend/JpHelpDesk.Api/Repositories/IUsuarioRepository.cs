using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Repositories;

public interface IUsuarioRepository
{
    Task<ResultadoPaginado<Usuario>> ListarAsync(UsuarioQuery query, CancellationToken cancellationToken);
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExisteAsync(string email, int? ignorarId, CancellationToken cancellationToken);
    Task<int> ContarAtivosPorPerfilAsync(PerfilUsuario perfil, CancellationToken cancellationToken);
    void Adicionar(Usuario usuario);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
