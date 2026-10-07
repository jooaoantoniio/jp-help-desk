using JpHelpDesk.Api.Data;
using JpHelpDesk.Api.Data.Extensions;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Repositories;

public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public async Task<ResultadoPaginado<Usuario>> ListarAsync(UsuarioQuery query, CancellationToken cancellationToken)
    {
        var consulta = context.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var busca = query.Busca.Trim();
            consulta = consulta.Where(u => u.Nome.Contains(busca) || u.Email.Contains(busca));
        }

        if (query.Perfil.HasValue)
        {
            consulta = consulta.Where(u => u.Perfil == query.Perfil.Value);
        }

        if (query.Ativo.HasValue)
        {
            consulta = consulta.Where(u => u.Ativo == query.Ativo.Value);
        }

        return await consulta
            .OrderBy(u => u.Nome)
            .ToResultadoPaginadoAsync(query, cancellationToken);
    }

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExisteAsync(string email, int? ignorarId, CancellationToken cancellationToken) =>
        context.Usuarios.AnyAsync(u => u.Email == email && u.Id != ignorarId, cancellationToken);

    public Task<int> ContarAtivosPorPerfilAsync(PerfilUsuario perfil, CancellationToken cancellationToken) =>
        context.Usuarios.CountAsync(u => u.Ativo && u.Perfil == perfil, cancellationToken);

    public void Adicionar(Usuario usuario) => context.Usuarios.Add(usuario);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
